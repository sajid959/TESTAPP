using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DSAGrind.Problems.API.Services;

namespace DSAGrind.Problems.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories()
    {
        try
        {
            var categories = await _categoryService.GetCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return StatusCode(500, new { message = "An error occurred while getting categories" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(string id)
    {
        try
        {
            var category = await _categoryService.GetCategoryAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            
            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category {CategoryId}", id);
            return StatusCode(500, new { message = "An error occurred while getting category" });
        }
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<CategoryDto>> GetCategoryBySlug(string slug)
    {
        try
        {
            var category = await _categoryService.GetCategoryBySlugAsync(slug);
            if (category == null)
            {
                return NotFound();
            }
            
            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category by slug {Slug}", slug);
            return StatusCode(500, new { message = "An error occurred while getting category" });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var category = await _categoryService.CreateCategoryAsync(request, userId);
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, new { message = "An error occurred while creating category" });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(string id, [FromBody] UpdateCategoryRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var category = await _categoryService.UpdateCategoryAsync(id, request, userId);
            if (category == null)
            {
                return NotFound();
            }
            
            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", id);
            return StatusCode(500, new { message = "An error occurred while updating category" });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpPost("reorder")]
    public async Task<IActionResult> UpdateCategoryOrder([FromBody] List<CategoryOrderDto> orders)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var success = await _categoryService.UpdateCategoryOrderAsync(orders, userId);
            if (!success)
            {
                return BadRequest(new { message = "Failed to update category order" });
            }
            
            return Ok(new { message = "Category order updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category order");
            return StatusCode(500, new { message = "An error occurred while updating category order" });
        }
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(string id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var success = await _categoryService.DeleteCategoryAsync(id, userId);
            if (!success)
            {
                return NotFound();
            }
            
            return Ok(new { message = "Category deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting category" });
        }
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}