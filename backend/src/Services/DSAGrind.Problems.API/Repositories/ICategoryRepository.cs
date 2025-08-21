using DSAGrind.Common.Repositories;
using DSAGrind.Models.Entities;

namespace DSAGrind.Problems.API.Repositories;

public interface ICategoryRepository : IMongoRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<Category>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateProblemCountAsync(string categoryId, int count, CancellationToken cancellationToken = default);
    Task<bool> UpdateMetadataAsync(string categoryId, CategoryMetadata metadata, CancellationToken cancellationToken = default);
}