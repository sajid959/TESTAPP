using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using DSAGrind.Common.Configuration;

namespace DSAGrind.Common.Repositories;

public class MongoRepository<T> : IMongoRepository<T> where T : class
{
    protected readonly IMongoCollection<T> _collection;
    protected readonly IMongoDatabase _database;

    public MongoRepository(IMongoDatabase database, string collectionName)
    {
        _database = database;
        _collection = database.GetCollection<T>(collectionName);
    }

    public virtual async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            return null;

        return await GetByIdAsync(objectId, cancellationToken);
    }

    public virtual async Task<T?> GetByIdAsync(ObjectId id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.Eq("_id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<T?> GetAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(_ => true).ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetManyAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public virtual async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        return entity;
    }

    public virtual async Task<IEnumerable<T>> CreateManyAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        var entitiesList = entities.ToList();
        if (entitiesList.Count == 0)
            return entitiesList;

        await _collection.InsertManyAsync(entitiesList, cancellationToken: cancellationToken);
        return entitiesList;
    }

    public virtual async Task<T?> UpdateAsync(string id, T entity, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            return null;

        return await UpdateAsync(objectId, entity, cancellationToken);
    }

    public virtual async Task<T?> UpdateAsync(ObjectId id, T entity, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.Eq("_id", id);
        var result = await _collection.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);
        return result.MatchedCount > 0 ? entity : null;
    }

    public virtual async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            return false;

        return await DeleteAsync(objectId, cancellationToken);
    }

    public virtual async Task<bool> DeleteAsync(ObjectId id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.Eq("_id", id);
        var result = await _collection.DeleteOneAsync(filter, cancellationToken);
        return result.DeletedCount > 0;
    }

    public virtual async Task<long> DeleteManyAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
    {
        var result = await _collection.DeleteManyAsync(filter, cancellationToken);
        return result.DeletedCount;
    }

    public virtual async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            return false;

        return await ExistsAsync(objectId, cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(ObjectId id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.Eq("_id", id);
        return await _collection.Find(filter).AnyAsync(cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(filter).AnyAsync(cancellationToken);
    }

    public virtual async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.CountDocumentsAsync(_ => true, cancellationToken: cancellationToken);
    }

    public virtual async Task<long> CountAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
    {
        return await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }

    public virtual async Task<PaginatedResult<T>> GetPagedAsync(
        int page, 
        int pageSize, 
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        var filterDefinition = filter != null ? Builders<T>.Filter.Where(filter) : Builders<T>.Filter.Empty;
        
        var totalCount = await _collection.CountDocumentsAsync(filterDefinition, cancellationToken: cancellationToken);
        
        var query = _collection.Find(filterDefinition);
        
        if (sortBy != null)
        {
            query = sortDescending 
                ? query.SortByDescending(sortBy)
                : query.SortBy(sortBy);
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public virtual async Task<IEnumerable<T>> SearchAsync(
        string searchTerm,
        Expression<Func<T, bool>>? filter = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var searchFilter = Builders<T>.Filter.Text(searchTerm);
        var combinedFilter = filter != null 
            ? Builders<T>.Filter.And(searchFilter, filter)
            : searchFilter;

        var query = _collection.Find(combinedFilter);
        
        if (limit.HasValue)
        {
            query = query.Limit(limit.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TResult>> AggregateAsync<TResult>(
        IEnumerable<BsonDocument> pipeline,
        CancellationToken cancellationToken = default)
    {
        return await _collection.Aggregate<TResult>(pipeline).ToListAsync(cancellationToken);
    }

    public virtual async Task<bool> UpdateOneAsync(
        Expression<Func<T, bool>> filter,
        Dictionary<string, object> updates,
        CancellationToken cancellationToken = default)
    {
        var updateDefinition = Builders<T>.Update.Combine(
            updates.Select(kvp => Builders<T>.Update.Set(kvp.Key, kvp.Value))
        );

        var result = await _collection.UpdateOneAsync(filter, updateDefinition, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }

    public virtual async Task<long> UpdateManyAsync(
        Expression<Func<T, bool>> filter,
        Dictionary<string, object> updates,
        CancellationToken cancellationToken = default)
    {
        var updateDefinition = Builders<T>.Update.Combine(
            updates.Select(kvp => Builders<T>.Update.Set(kvp.Key, kvp.Value))
        );

        var result = await _collection.UpdateManyAsync(filter, updateDefinition, cancellationToken: cancellationToken);
        return result.ModifiedCount;
    }

    public virtual async Task<bool> BulkWriteAsync(
        IEnumerable<(T entity, BulkOperationType operation)> operations,
        CancellationToken cancellationToken = default)
    {
        var writeModels = new List<WriteModel<T>>();

        foreach (var (entity, operation) in operations)
        {
            WriteModel<T> writeModel = operation switch
            {
                BulkOperationType.Insert => new InsertOneModel<T>(entity),
                BulkOperationType.Update => new ReplaceOneModel<T>(Builders<T>.Filter.Eq("_id", GetEntityId(entity)), entity),
                BulkOperationType.Delete => new DeleteOneModel<T>(Builders<T>.Filter.Eq("_id", GetEntityId(entity))),
                BulkOperationType.Upsert => new ReplaceOneModel<T>(Builders<T>.Filter.Eq("_id", GetEntityId(entity)), entity) { IsUpsert = true },
                _ => throw new ArgumentException($"Unsupported bulk operation type: {operation}")
            };

            writeModels.Add(writeModel);
        }

        if (writeModels.Count == 0)
            return true;

        var result = await _collection.BulkWriteAsync(writeModels, cancellationToken: cancellationToken);
        return result.IsAcknowledged;
    }

    protected virtual object GetEntityId(T entity)
    {
        var idProperty = typeof(T).GetProperty("Id");
        return idProperty?.GetValue(entity) ?? throw new InvalidOperationException("Entity must have an Id property");
    }
}