using HospitalTriageAI.Models.Enums;

namespace HospitalTriageAI.Models;

/// <summary>
/// Department prediction result from the analysis system
/// </summary>
public class DepartmentPrediction
{
    public int Id { get; set; }
    
    public int PatientId { get; set; }
    
    public int? AssessmentId { get; set; }
    
    /// <summary>
    /// Recommended department based on analysis
    /// </summary>
    public Department RecommendedDepartment { get; set; }
    
    /// <summary>
    /// Confidence score (0-100)
    /// </summary>
    public int ConfidenceScore { get; set; }
    
    /// <summary>
    /// Clinical explanation summary
    /// </summary>
    public string ClinicalExplanation { get; set; } = string.Empty;
    
    /// <summary>
    /// JSON-serialized department scores for all departments
    /// </summary>
    public string? DepartmentScoresJson { get; set; }
    
    /// <summary>
    /// Indicates if this was an emergency-priority decision
    /// </summary>
    public bool IsEmergencyPriority { get; set; }
    
    /// <summary>
    /// Key clinical findings that influenced the decision
    /// </summary>
    public string? KeyFindings { get; set; }
    
    /// <summary>
    /// Timestamp of prediction
    /// </summary>
    public DateTime PredictedAt { get; set; } = DateTime.Now;
    
    // Navigation properties
    public Patient? Patient { get; set; }
    public TriageAssessment? Assessment { get; set; }
}

/// <summary>
/// Detailed department score breakdown
/// </summary>
public class DepartmentScore
{
    public Department Department { get; set; }
    public int Score { get; set; }
    public List<string> ContributingFactors { get; set; } = new();
}

/// <summary>
/// Result of department analysis
/// </summary>
public class DepartmentAnalysisResult
{
    public Department RecommendedDepartment { get; set; }
    public int ConfidenceScore { get; set; }
    public string ClinicalExplanation { get; set; } = string.Empty;
    public List<DepartmentScore> AllScores { get; set; } = new();
    public bool IsEmergencyPriority { get; set; }
    public List<string> KeyFindings { get; set; } = new();
}
