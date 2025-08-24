using DSAGrind.Models.DTOs;

namespace DSAGrind.MCP.API.Services;

public interface IMCPService
{
    Task<InitializeResponseDto> InitializeAsync(InitializeRequestDto request);
    Task<List<ToolDto>> ListToolsAsync();
    Task<ToolCallResponseDto> CallToolAsync(ToolCallRequestDto request);
    Task<List<PromptDto>> ListPromptsAsync();
    Task<GetPromptResponseDto> GetPromptAsync(GetPromptRequestDto request);
    Task<List<ResourceDto>> ListResourcesAsync();
    Task<ReadResourceResponseDto> ReadResourceAsync(ReadResourceRequestDto request);
}