namespace DSAGrind.Common.Configuration;

public class QdrantSettings
{
    public const string SectionName = "QdrantSettings";

    public string Url { get; set; } = "http://localhost:6333";
    public string ApiKey { get; set; } = string.Empty;
    public string CollectionName { get; set; } = "dsagrind_problems";
    public int VectorSize { get; set; } = 1536; // OpenAI ada-002 embedding size
    public string Distance { get; set; } = "Cosine"; // Cosine, Euclid, Dot
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
    public bool UseGrpcInterface { get; set; } = false;
    public int GrpcPort { get; set; } = 6334;
    
    // Search settings
    public int DefaultSearchLimit { get; set; } = 10;
    public double MinSimilarityScore { get; set; } = 0.7;
    public bool EnableFiltering { get; set; } = true;
    
    // Indexing settings
    public int BatchSize { get; set; } = 100;
    public int IndexingConcurrency { get; set; } = 4;
    public bool AutoCreateCollection { get; set; } = true;
    
    // Performance settings
    public int MemMapThreshold { get; set; } = 20000;
    public int OnDiskPayload { get; set; } = 10000;
    public bool EnableQuantization { get; set; } = false;
}