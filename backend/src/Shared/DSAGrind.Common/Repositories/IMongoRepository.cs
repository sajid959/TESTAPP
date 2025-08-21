using System.Linq.Expressions;
using MongoDB.Bson;

namespace DSAGrind.Common.Repositories;

public interface IMongoRepository<T> where T : class
{
    // Basic CRUD operations
    Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync(ObjectId id, CancellationToken cancellationToken = default);
    Task<T?> GetAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetManyAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> CreateManyAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task<T?> UpdateAsync(string id, T entity, CancellationToken cancellationToken = default);
    Task<T?> UpdateAsync(ObjectId id, T entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(ObjectId id, CancellationToken cancellationToken = default);
    Task<long> DeleteManyAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);

    // Existence checks
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(ObjectId id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);

    // Counting
    Task<long> CountAsync(CancellationToken cancellationToken = default);
    Task<long> CountAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);

    // Pagination
    Task<PaginatedResult<T>> GetPagedAsync(
        int page, 
        int pageSize, 
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default);

    // Search
    Task<IEnumerable<T>> SearchAsync(
        string searchTerm,
        Expression<Func<T, bool>>? filter = null,
        int? limit = null,
        CancellationToken cancellationToken = default);

    // Aggregation
    Task<IEnumerable<TResult>> AggregateAsync<TResult>(
        IEnumerable<BsonDocument> pipeline,
        CancellationToken cancellationToken = default);

    // Update operations
    Task<bool> UpdateOneAsync(
        Expression<Func<T, bool>> filter,
        Dictionary<string, object> updates,
        CancellationToken cancellationToken = default);

    Task<long> UpdateManyAsync(
        Expression<Func<T, bool>> filter,
        Dictionary<string, object> updates,
        CancellationToken cancellationToken = default);

    // Bulk operations
    Task<bool> BulkWriteAsync(
        IEnumerable<(T entity, BulkOperationType operation)> operations,
        CancellationToken cancellationToken = default);
}

public class PaginatedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public long TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

public enum BulkOperationType
{
    Insert,
    Update,
    Delete,
    Upsert
}