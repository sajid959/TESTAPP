namespace DSAGrind.Common.Configuration;

public class AISettings
{
    public const string SectionName = "AISettings";

    public AIProvider DefaultProvider { get; set; } = AIProvider.Perplexity;
    
    // OpenAI Settings
    public string OpenAIApiKey { get; set; } = string.Empty;
    public string OpenAIModel { get; set; } = "gpt-3.5-turbo";
    public string OpenAIBaseUrl { get; set; } = "https://api.openai.com/v1";
    
    // Perplexity Settings
    public string PerplexityApiKey { get; set; } = string.Empty;
    public string PerplexityModel { get; set; } = "llama-3.1-sonar-small-128k-online";
    public string PerplexityBaseUrl { get; set; } = "https://api.perplexity.ai";
    
    // Anthropic Settings
    public string AnthropicApiKey { get; set; } = string.Empty;
    public string AnthropicModel { get; set; } = "claude-3-haiku-20240307";
    public string AnthropicBaseUrl { get; set; } = "https://api.anthropic.com";
    
    // Google Settings
    public string GoogleApiKey { get; set; } = string.Empty;
    public string GoogleModel { get; set; } = "gemini-pro";
    public string GoogleBaseUrl { get; set; } = "https://generativelanguage.googleapis.com";
    
    // Local AI Settings
    public string LocalBaseUrl { get; set; } = "http://localhost:11434";
    public string LocalModel { get; set; } = "llama2";
    
    // Common Settings
    public int MaxTokens { get; set; } = 2000;
    public double Temperature { get; set; } = 0.2;
    public double TopP { get; set; } = 0.9;
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryAttempts { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 1000;
    
    // Rate limiting
    public int RequestsPerMinute { get; set; } = 60;
    public int RequestsPerDay { get; set; } = 1000;
    
    // Features
    public bool EnableCodeAnalysis { get; set; } = true;
    public bool EnableHintGeneration { get; set; } = true;
    public bool EnableTestCaseGeneration { get; set; } = true;
    public bool EnableDifficultyEstimation { get; set; } = true;
    public bool EnableDebugAssistance { get; set; } = true;
}

public enum AIProvider
{
    OpenAI,
    Perplexity,
    Anthropic,
    Google,
    Local
}