using MongoDB.Driver;
using Microsoft.AspNetCore.Mvc;

namespace DSAGrind.Problems.API.Controllers;

[Route("api/[controller]")]
[ApiController] 
public class TestUserController : ControllerBase
{
    private readonly IMongoDatabase _database;
    
    public TestUserController(IMongoDatabase database)
    {
        _database = database;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> RegisterTestUser()
    {
        try
        {
            var usersCollection = _database.GetCollection<dynamic>("users");
            
            var testUser = new 
            {
                _id = "test_user_" + DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                username = "realcloudtest",
                email = "cloudtest@dsagrind.com",
                firstName = "Real",
                lastName = "CloudUser", 
                role = "User",
                isEmailVerified = false,
                subscriptionPlan = "Free",
                subscriptionStatus = "Active",
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow
            };
            
            await usersCollection.InsertOneAsync(testUser);
            
            var userCount = await usersCollection.CountDocumentsAsync(Builders<dynamic>.Filter.Empty);
            
            return Ok(new
            {
                message = "✅ User successfully registered to MongoDB Atlas cloud database!",
                userId = testUser._id,
                username = testUser.username,
                email = testUser.email,
                totalUsersInDatabase = userCount,
                databaseConnection = "MongoDB Atlas Cloud",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = "❌ Failed to register user to MongoDB Atlas",
                error = ex.Message
            });
        }
    }
    
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var usersCollection = _database.GetCollection<dynamic>("users");
            var users = await usersCollection.Find(Builders<dynamic>.Filter.Empty).ToListAsync();
            var userCount = users.Count;
            
            return Ok(new
            {
                message = "✅ Users retrieved from MongoDB Atlas cloud database",
                totalUsers = userCount,
                users = users.Take(5).ToArray(), // Show first 5 users
                databaseConnection = "MongoDB Atlas Cloud"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = "❌ Failed to retrieve users from MongoDB Atlas",
                error = ex.Message
            });
        }
    }
}
