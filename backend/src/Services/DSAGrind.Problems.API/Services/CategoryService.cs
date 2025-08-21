using DSAGrind.Problems.API.Repositories;
using DSAGrind.Models.Entities;
using DSAGrind.Common.Services;
using AutoMapper;

namespace DSAGrind.Problems.API.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        ICategoryRepository categoryRepository,
        IEventPublisher eventPublisher,
        IMapper mapper,
        ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _eventPublisher = eventPublisher;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetActiveAsync(cancellationToken);
        return _mapper.Map<List<CategoryDto>>(categories);
    }

    public async Task<CategoryDto?> GetCategoryAsync(string id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        return category == null ? null : _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto?> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetBySlugAsync(slug, cancellationToken);
        return category == null ? null : _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequestDto request, string userId, CancellationToken cancellationToken = default)
    {
        var category = new Category
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Slug = GenerateSlug(request.Name),
            Description = request.Description,
            Icon = request.Icon,
            IsPaid = request.IsPaid,
            FreeProblemLimit = request.FreeProblemLimit,
            TotalProblems = 0,
            IsActive = true,
            OrderIndex = 999,
            Metadata = new CategoryMetadata
            {
                Tags = request.Tags,
                Difficulty = new DifficultyDistribution()
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _categoryRepository.CreateAsync(category, cancellationToken);

        // Publish event
        await _eventPublisher.PublishAsync("category.created", new { CategoryId = category.Id, UserId = userId }, cancellationToken);

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(string id, UpdateCategoryRequestDto request, string userId, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category == null) return null;

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.Name))
        {
            category.Name = request.Name;
            category.Slug = GenerateSlug(request.Name);
        }

        if (request.Description != null)
            category.Description = request.Description;

        if (request.Icon != null)
            category.Icon = request.Icon;

        if (request.IsPaid.HasValue)
            category.IsPaid = request.IsPaid.Value;

        if (request.FreeProblemLimit.HasValue)
            category.FreeProblemLimit = request.FreeProblemLimit.Value;

        if (request.IsActive.HasValue)
            category.IsActive = request.IsActive.Value;

        if (request.Tags != null)
            category.Metadata.Tags = request.Tags;

        category.UpdatedAt = DateTime.UtcNow;

        await _categoryRepository.UpdateAsync(category, cancellationToken);

        // Publish event
        await _eventPublisher.PublishAsync("category.updated", new { CategoryId = category.Id, UserId = userId }, cancellationToken);

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<bool> DeleteCategoryAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var success = await _categoryRepository.DeleteAsync(id, cancellationToken);

        if (success)
        {
            await _eventPublisher.PublishAsync("category.deleted", new { CategoryId = id, UserId = userId }, cancellationToken);
        }

        return success;
    }

    public async Task<bool> UpdateCategoryOrderAsync(List<CategoryOrderDto> orders, string userId, CancellationToken cancellationToken = default)
    {
        foreach (var order in orders)
        {
            var category = await _categoryRepository.GetByIdAsync(order.Id, cancellationToken);
            if (category != null)
            {
                category.OrderIndex = order.OrderIndex;
                category.UpdatedAt = DateTime.UtcNow;
                await _categoryRepository.UpdateAsync(category, cancellationToken);
            }
        }

        // Publish event
        await _eventPublisher.PublishAsync("categories.reordered", new { UserId = userId }, cancellationToken);

        return true;
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace("(", "")
            .Replace(")", "");
    }
}