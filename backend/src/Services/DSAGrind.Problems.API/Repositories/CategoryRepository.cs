using DSAGrind.Common.Repositories;
using DSAGrind.Models.Entities;
using MongoDB.Driver;

namespace DSAGrind.Problems.API.Repositories;

public class CategoryRepository : MongoRepository<Category>, ICategoryRepository
{
    public CategoryRepository(IMongoDatabase database) : base(database, "categories")
    {
    }

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await Collection.Find(c => c.Slug == slug).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Category>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await Collection
            .Find(c => c.IsActive)
            .Sort(Builders<Category>.Sort.Ascending(c => c.OrderIndex))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UpdateProblemCountAsync(string categoryId, int count, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Category>.Filter.Eq(c => c.Id, categoryId);
        var update = Builders<Category>.Update.Set(c => c.TotalProblems, count);
        
        var result = await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> UpdateMetadataAsync(string categoryId, CategoryMetadata metadata, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Category>.Filter.Eq(c => c.Id, categoryId);
        var update = Builders<Category>.Update.Set(c => c.Metadata, metadata);
        
        var result = await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }
}