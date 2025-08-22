using System.ComponentModel.DataAnnotations;

namespace DSAGrind.Models.DTOs;

public class AIHintRequestDto
{
    [Required]
    public string ProblemId { get; set; } = string.Empty;
    
    public string? UserCode { get; set; }
    public string? Language { get; set; }
    public string? SpecificQuestion { get; set; }
    public string? ErrorMessage { get; set; }
    public AIHintLevel HintLevel { get; set; } = AIHintLevel.Gentle;
}

public class AIHintResponseDto
{
    public string Hint { get; set; } = string.Empty;
    public string HintType { get; set; } = string.Empty;
    public AIHintLevel Level { get; set; }
    public List<string> SuggestedApproaches { get; set; } = new();
    public List<string> KeyConcepts { get; set; } = new();
    public string? CodeSuggestion { get; set; }
    public bool IsComplete { get; set; }
}

public class AICodeReviewRequestDto
{
    [Required]
    public string ProblemId { get; set; } = string.Empty;
    
    [Required]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    public string Language { get; set; } = string.Empty;
    
    public List<string>? TestResults { get; set; }
    public string? ErrorMessage { get; set; }
}

public class AICodeReviewResponseDto
{
    public string OverallFeedback { get; set; } = string.Empty;
    public AICodeQuality QualityScore { get; set; } = new();
    public List<AICodeSuggestionDto> Suggestions { get; set; } = new();
    public List<string> PositiveAspects { get; set; } = new();
    public List<string> ImprovementAreas { get; set; } = new();
    public TimeSpaceComplexityDto Complexity { get; set; } = new();
}

public class AICodeSuggestionDto
{
    public string Type { get; set; } = string.Empty; // "optimization", "bug", "style", "logic"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? CodeExample { get; set; }
    public int Priority { get; set; } // 1-5, 5 being highest
    public int? LineNumber { get; set; }
}

public class AICodeQuality
{
    public int Overall { get; set; } // 1-100
    public int Readability { get; set; } // 1-100
    public int Efficiency { get; set; } // 1-100
    public int Correctness { get; set; } // 1-100
    public int Style { get; set; } // 1-100
}

public class TimeSpaceComplexityDto
{
    public string TimeComplexity { get; set; } = string.Empty;
    public string SpaceComplexity { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public bool IsOptimal { get; set; }
    public string? OptimalComplexity { get; set; }
}

public class AIProblemExplanationRequestDto
{
    [Required]
    public string ProblemId { get; set; } = string.Empty;
    
    public ExplanationLevel Level { get; set; } = ExplanationLevel.Intermediate;
    public string? SpecificTopic { get; set; }
}

public class AIProblemExplanationResponseDto
{
    public string ProblemBreakdown { get; set; } = string.Empty;
    public List<string> KeyInsights { get; set; } = new();
    public List<AIApproachDto> Approaches { get; set; } = new();
    public List<string> Prerequisites { get; set; } = new();
    public List<string> SimilarProblems { get; set; } = new();
    public string DifficultyAnalysis { get; set; } = string.Empty;
}

public class AIApproachDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TimeComplexity { get; set; } = string.Empty;
    public string SpaceComplexity { get; set; } = string.Empty;
    public List<string> Steps { get; set; } = new();
    public string? PseudoCode { get; set; }
    public int Difficulty { get; set; } // 1-5
}

public class AILearningPathRequestDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public List<string>? CompletedProblems { get; set; }
    public List<string>? WeakAreas { get; set; }
    public string? TargetLevel { get; set; } // "beginner", "intermediate", "advanced"
    public string? TimeCommitment { get; set; } // "casual", "moderate", "intensive"
}

public class AILearningPathResponseDto
{
    public string PathName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<AILearningStepDto> Steps { get; set; } = new();
    public List<string> RecommendedProblems { get; set; } = new();
    public string EstimatedTimeToComplete { get; set; } = string.Empty;
    public List<string> KeyTopics { get; set; } = new();
}

public class AILearningStepDto
{
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Problems { get; set; } = new();
    public List<string> Resources { get; set; } = new();
    public string EstimatedTime { get; set; } = string.Empty;
}

public class AICodeGenerationRequestDto
{
    [Required]
    public string ProblemId { get; set; } = string.Empty;
    
    [Required]
    public string Language { get; set; } = string.Empty;
    
    public string? Approach { get; set; }
    public bool IncludeComments { get; set; } = true;
    public bool IncludeTestCases { get; set; } = false;
    public CodeStyle Style { get; set; } = CodeStyle.Clean;
}

public class AICodeGenerationResponseDto
{
    public string Code { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public TimeSpaceComplexityDto Complexity { get; set; } = new();
    public List<string> KeyPoints { get; set; } = new();
    public string? TestCases { get; set; }
    public List<string> AlternativeApproaches { get; set; } = new();
}

public enum AIHintLevel
{
    Gentle = 1,
    Moderate = 2,
    Direct = 3,
    Detailed = 4,
    Solution = 5
}

public enum ExplanationLevel
{
    Beginner = 1,
    Intermediate = 2,
    Advanced = 3,
    Expert = 4
}

public enum CodeStyle
{
    Clean = 1,
    Verbose = 2,
    Concise = 3,
    Educational = 4
}