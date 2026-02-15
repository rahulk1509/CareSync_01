namespace HospitalTriageAI.Services;

/// <summary>
/// Results of bias/fairness analysis on training data
/// </summary>
public class BiasAnalysisResult
{
    public bool IsAvailable { get; set; }
    public int TotalRecords { get; set; }
    public DateTime? AnalysisDate { get; set; }
    
    // Gender metrics
    public GenderMetrics? GenderAnalysis { get; set; }
    
    // Age group metrics
    public List<AgeGroupMetrics> AgeGroupAnalysis { get; set; } = new();
    
    // Overall fairness score (0-100)
    public double FairnessScore { get; set; }
    public string FairnessRating { get; set; } = "Unknown";
}

/// <summary>
/// Accuracy and error metrics by gender
/// </summary>
public class GenderMetrics
{
    public DemographicAccuracy Male { get; set; } = new();
    public DemographicAccuracy Female { get; set; } = new();
    
    // Disparity metrics
    public double AccuracyDisparity => Math.Abs(Male.Accuracy - Female.Accuracy);
    public double FalsePositiveDisparity => Math.Abs(Male.FalsePositiveRate - Female.FalsePositiveRate);
    public double FalseNegativeDisparity => Math.Abs(Male.FalseNegativeRate - Female.FalseNegativeRate);
}

/// <summary>
/// Metrics for a specific demographic group
/// </summary>
public class DemographicAccuracy
{
    public string Group { get; set; } = string.Empty;
    public int SampleCount { get; set; }
    public double Accuracy { get; set; }
    public double FalsePositiveRate { get; set; }
    public double FalseNegativeRate { get; set; }
    
    // Risk distribution
    public int LowRiskCount { get; set; }
    public int MediumRiskCount { get; set; }
    public int HighRiskCount { get; set; }
    public int CriticalRiskCount { get; set; }
}

/// <summary>
/// Risk distribution and metrics by age group
/// </summary>
public class AgeGroupMetrics
{
    public string AgeGroup { get; set; } = string.Empty;
    public int MinAge { get; set; }
    public int MaxAge { get; set; }
    public int SampleCount { get; set; }
    public double Accuracy { get; set; }
    public double FalsePositiveRate { get; set; }
    public double FalseNegativeRate { get; set; }
    
    // Risk distribution
    public int LowRiskCount { get; set; }
    public int MediumRiskCount { get; set; }
    public int HighRiskCount { get; set; }
    public int CriticalRiskCount { get; set; }
    
    // Percentage distribution
    public double LowRiskPercent => SampleCount > 0 ? (double)LowRiskCount / SampleCount * 100 : 0;
    public double MediumRiskPercent => SampleCount > 0 ? (double)MediumRiskCount / SampleCount * 100 : 0;
    public double HighRiskPercent => SampleCount > 0 ? (double)HighRiskCount / SampleCount * 100 : 0;
    public double CriticalRiskPercent => SampleCount > 0 ? (double)CriticalRiskCount / SampleCount * 100 : 0;
}
