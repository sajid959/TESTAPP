namespace DSAGrind.Models.DTOs;

// Request/Response DTOs
public class InitializeRequestDto
{
    public string ProtocolVersion { get; set; } = "2024-11-05";
    public ClientInfoDto? ClientInfo { get; set; }
    public ClientCapabilitiesDto? Capabilities { get; set; }
}

public class InitializeResponseDto
{
    public string ProtocolVersion { get; set; } = "2024-11-05";
    public ServerInfoDto ServerInfo { get; set; } = new();
    public ServerCapabilitiesDto Capabilities { get; set; } = new();
}

public class ToolCallRequestDto
{
    public string Name { get; set; } = string.Empty;
    public object Arguments { get; set; } = new();
}

public class ToolCallResponseDto
{
    public List<ContentDto> Content { get; set; } = new();
}

public class GetPromptRequestDto
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object>? Arguments { get; set; }
}

public class GetPromptResponseDto
{
    public List<PromptMessageDto> Messages { get; set; } = new();
}

public class ReadResourceRequestDto
{
    public string Uri { get; set; } = string.Empty;
}

public class ReadResourceResponseDto
{
    public List<ContentDto> Contents { get; set; } = new();
}

// Info DTOs
public class ClientInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

public class ServerInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

// Capability DTOs
public class ClientCapabilitiesDto
{
    public SamplingCapabilityDto? Sampling { get; set; }
}

public class ServerCapabilitiesDto
{
    public ToolsCapabilityDto? Tools { get; set; }
    public PromptsCapabilityDto? Prompts { get; set; }
    public ResourcesCapabilityDto? Resources { get; set; }
    public LoggingCapabilityDto? Logging { get; set; }
}

public class SamplingCapabilityDto
{
}

public class ToolsCapabilityDto
{
    public bool ListChanged { get; set; }
}

public class PromptsCapabilityDto
{
    public bool ListChanged { get; set; }
}

public class ResourcesCapabilityDto
{
    public bool Subscribe { get; set; }
    public bool ListChanged { get; set; }
}

public class LoggingCapabilityDto
{
}

// Tool DTOs
public class ToolDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public object InputSchema { get; set; } = new();
}

// Prompt DTOs
public class PromptDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<PromptArgumentDto> Arguments { get; set; } = new();
}

public class PromptArgumentDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Required { get; set; }
}

public class PromptMessageDto
{
    public string Role { get; set; } = string.Empty;
    public ContentDto Content { get; set; } = new();
}

// Resource DTOs
public class ResourceDto
{
    public string Uri { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
}

// Content DTOs
public class ContentDto
{
    public string Type { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}