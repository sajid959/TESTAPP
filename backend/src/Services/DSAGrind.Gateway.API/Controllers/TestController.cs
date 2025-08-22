using Microsoft.AspNetCore.Mvc;

namespace DSAGrind.Gateway.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { 
            status = "healthy", 
            service = "DSAGrind Test API", 
            timestamp = DateTime.UtcNow,
            message = "All systems operational!" 
        });
    }

    [HttpGet("categories")]
    public IActionResult GetCategories()
    {
        var categories = new[]
        {
            new { id = "1", name = "Arrays & Hashing", slug = "arrays-hashing", freeQuestionLimit = 5, totalQuestions = 15 },
            new { id = "2", name = "Two Pointers", slug = "two-pointers", freeQuestionLimit = 3, totalQuestions = 10 },
            new { id = "3", name = "Binary Search", slug = "binary-search", freeQuestionLimit = 3, totalQuestions = 12 },
            new { id = "4", name = "Trees", slug = "trees", freeQuestionLimit = 4, totalQuestions = 20 },
            new { id = "5", name = "Dynamic Programming", slug = "dynamic-programming", freeQuestionLimit = 2, totalQuestions = 25 }
        };
        
        return Ok(categories);
    }

    [HttpGet("problems")]
    public IActionResult GetProblems()
    {
        var problems = new[]
        {
            new { 
                id = "1", 
                title = "Two Sum", 
                slug = "two-sum",
                difficulty = "Easy", 
                description = "Given an array of integers nums and an integer target, return indices of the two numbers such that they add up to target.",
                isPremium = false,
                category = "Arrays & Hashing"
            },
            new { 
                id = "2", 
                title = "Valid Palindrome", 
                slug = "valid-palindrome",
                difficulty = "Easy", 
                description = "A phrase is a palindrome if, after converting all uppercase letters into lowercase letters and removing all non-alphanumeric characters, it reads the same forward and backward.",
                isPremium = false,
                category = "Two Pointers"
            },
            new { 
                id = "3", 
                title = "Binary Search", 
                slug = "binary-search",
                difficulty = "Easy", 
                description = "Given an array of integers nums which is sorted in ascending order, and an integer target, write a function to search target in nums.",
                isPremium = false,
                category = "Binary Search"
            }
        };
        
        return Ok(problems);
    }

    [HttpPost("auth/login")]
    public IActionResult Login([FromBody] object loginData)
    {
        return Ok(new { 
            message = "Login endpoint working",
            token = "test-jwt-token",
            user = new { id = "1", email = "test@example.com", username = "testuser" }
        });
    }

    [HttpPost("auth/register")]
    public IActionResult Register([FromBody] object registerData)
    {
        return Ok(new { 
            message = "Registration endpoint working",
            token = "test-jwt-token",
            user = new { id = "1", email = "test@example.com", username = "testuser" }
        });
    }
}