using DSAGrind.Common.Repositories;
using DSAGrind.Models.Entities;
using DSAGrind.Models.DTOs;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Options;
using DSAGrind.Common.Configuration;

namespace DSAGrind.Problems.API.Repositories;

public class ProblemRepository : MongoRepository<Problem>, IProblemRepository
{
    public ProblemRepository(IOptions<MongoDbSettings> settings) : base(settings, "problems")
    {
    }

    public async Task<Problem?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(p => p.Slug == slug).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PaginatedResult<Problem>> GetByCategoryAsync(string categoryId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Problem>.Filter.Eq(p => p.CategoryId, categoryId);
        var totalCount = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        
        var problems = await _collection
            .Find(filter)
            .Sort(Builders<Problem>.Sort.Ascending(p => p.OrderIndex))
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<Problem>
        {
            Items = problems,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PaginatedResult<Problem>> SearchProblemsAsync(ProblemSearchRequestDto request, CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<Problem>.Filter;
        var filters = new List<FilterDefinition<Problem>>();

        // Text search
        if (!string.IsNullOrEmpty(request.Search))
        {
            filters.Add(filterBuilder.Or(
                filterBuilder.Regex(p => p.Title, new BsonRegularExpression(request.Search, "i")),
                filterBuilder.Regex(p => p.Description, new BsonRegularExpression(request.Search, "i"))
            ));
        }

        // Difficulty filter
        if (!string.IsNullOrEmpty(request.Difficulty))
        {
            filters.Add(filterBuilder.Eq(p => p.Difficulty, request.Difficulty));
        }

        // Category filter
        if (!string.IsNullOrEmpty(request.CategoryId))
        {
            filters.Add(filterBuilder.Eq(p => p.CategoryId, request.CategoryId));
        }

        // Tags filter
        if (request.Tags?.Any() == true)
        {
            filters.Add(filterBuilder.AnyIn(p => p.Tags, request.Tags));
        }

        // Premium filter
        if (request.IsPaid.HasValue)
        {
            filters.Add(filterBuilder.Eq(p => p.IsPaid, request.IsPaid.Value));
        }

        // Status filter (only approved problems for non-admins)
        filters.Add(filterBuilder.Eq(p => p.Status, "approved"));

        var combinedFilter = filters.Count > 0 ? filterBuilder.And(filters) : FilterDefinition<Problem>.Empty;
        
        var totalCount = await _collection.CountDocumentsAsync(combinedFilter, cancellationToken: cancellationToken);

        // Build sort
        var sortBuilder = Builders<Problem>.Sort;
        SortDefinition<Problem> sort = request.SortBy?.ToLower() switch
        {
            "difficulty" => request.SortOrder == "desc" ? sortBuilder.Descending(p => p.Difficulty) : sortBuilder.Ascending(p => p.Difficulty),
            "title" => request.SortOrder == "desc" ? sortBuilder.Descending(p => p.Title) : sortBuilder.Ascending(p => p.Title),
            "createdat" => request.SortOrder == "desc" ? sortBuilder.Descending(p => p.CreatedAt) : sortBuilder.Ascending(p => p.CreatedAt),
            "popularity" => sortBuilder.Descending(p => p.Statistics.TotalSubmissions),
            _ => sortBuilder.Ascending(p => p.OrderIndex)
        };

        var problems = await Collection
            .Find(combinedFilter)
            .Sort(sort)
            .Skip((request.Page - 1) * request.PageSize)
            .Limit(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<Problem>
        {
            Items = problems,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }

    public async Task<List<Problem>> GetRandomProblemsAsync(int count, string? difficulty = null, CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<Problem>.Filter;
        var filters = new List<FilterDefinition<Problem>>
        {
            filterBuilder.Eq(p => p.Status, "approved")
        };

        if (!string.IsNullOrEmpty(difficulty))
        {
            filters.Add(filterBuilder.Eq(p => p.Difficulty, difficulty));
        }

        var combinedFilter = filterBuilder.And(filters);

        return await Collection
            .Aggregate()
            .Match(combinedFilter)
            .Sample(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Problem>> GetRecommendedProblemsAsync(string userId, int count, CancellationToken cancellationToken = default)
    {
        // For now, return random problems. In a real implementation, this would use ML models
        return await GetRandomProblemsAsync(count, cancellationToken: cancellationToken);
    }

    public async Task<bool> UpdateStatisticsAsync(string problemId, ProblemStatistics statistics, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Problem>.Filter.Eq(p => p.Id, problemId);
        var update = Builders<Problem>.Update.Set(p => p.Statistics, statistics);
        
        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> IncrementViewAsync(string problemId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Problem>.Filter.Eq(p => p.Id, problemId);
        var update = Builders<Problem>.Update.Inc("Statistics.Views", 1);
        
        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> UpdateDifficultyRatingAsync(string problemId, string difficulty, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Problem>.Filter.Eq(p => p.Id, problemId);
        var update = Builders<Problem>.Update.Set(p => p.Difficulty, difficulty);
        
        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }

    public async Task<List<Problem>> GetSimilarProblemsAsync(string problemId, int count, CancellationToken cancellationToken = default)
    {
        var problem = await GetByIdAsync(problemId, cancellationToken);
        if (problem == null) return new List<Problem>();

        var filter = Builders<Problem>.Filter.And(
            Builders<Problem>.Filter.Ne(p => p.Id, problemId),
            Builders<Problem>.Filter.Eq(p => p.Status, "approved"),
            Builders<Problem>.Filter.AnyIn(p => p.Tags, problem.Tags)
        );

        return await Collection
            .Find(filter)
            .Limit(count)
            .ToListAsync(cancellationToken);
    }
}