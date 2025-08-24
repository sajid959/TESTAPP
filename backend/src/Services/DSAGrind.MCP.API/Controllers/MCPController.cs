using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DSAGrind.MCP.API.Services;
using DSAGrind.Models.DTOs;

namespace DSAGrind.MCP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MCPController : ControllerBase
{
    private readonly IMCPService _mcpService;
    private readonly ILogger<MCPController> _logger;

    public MCPController(IMCPService mcpService, ILogger<MCPController> logger)
    {
        _mcpService = mcpService;
        _logger = logger;
    }

    [HttpPost("initialize")]
    public async Task<IActionResult> Initialize([FromBody] InitializeRequestDto request)
    {
        try
        {
            _logger.LogInformation("Initializing MCP server for client: {ClientInfo}", request.ClientInfo?.Name);
            var response = await _mcpService.InitializeAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing MCP server");
            return StatusCode(500, new { error = "Failed to initialize MCP server" });
        }
    }

    [HttpPost("tools/list")]
    public async Task<IActionResult> ListTools()
    {
        try
        {
            var tools = await _mcpService.ListToolsAsync();
            return Ok(new { tools });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing MCP tools");
            return StatusCode(500, new { error = "Failed to list tools" });
        }
    }

    [HttpPost("tools/call")]
    public async Task<IActionResult> CallTool([FromBody] ToolCallRequestDto request)
    {
        try
        {
            _logger.LogInformation("Calling tool: {ToolName}", request.Name);
            var result = await _mcpService.CallToolAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling tool: {ToolName}", request.Name);
            return StatusCode(500, new { error = $"Failed to call tool: {request.Name}" });
        }
    }

    [HttpPost("prompts/list")]
    public async Task<IActionResult> ListPrompts()
    {
        try
        {
            var prompts = await _mcpService.ListPromptsAsync();
            return Ok(new { prompts });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing MCP prompts");
            return StatusCode(500, new { error = "Failed to list prompts" });
        }
    }

    [HttpPost("prompts/get")]
    public async Task<IActionResult> GetPrompt([FromBody] GetPromptRequestDto request)
    {
        try
        {
            var prompt = await _mcpService.GetPromptAsync(request);
            return Ok(prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting prompt: {PromptName}", request.Name);
            return StatusCode(500, new { error = $"Failed to get prompt: {request.Name}" });
        }
    }

    [HttpPost("resources/list")]
    public async Task<IActionResult> ListResources()
    {
        try
        {
            var resources = await _mcpService.ListResourcesAsync();
            return Ok(new { resources });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing MCP resources");
            return StatusCode(500, new { error = "Failed to list resources" });
        }
    }

    [HttpPost("resources/read")]
    public async Task<IActionResult> ReadResource([FromBody] ReadResourceRequestDto request)
    {
        try
        {
            var resource = await _mcpService.ReadResourceAsync(request);
            return Ok(resource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading resource: {ResourceUri}", request.Uri);
            return StatusCode(500, new { error = $"Failed to read resource: {request.Uri}" });
        }
    }
}