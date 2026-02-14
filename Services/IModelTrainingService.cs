namespace HospitalTriageAI.Services;

/// <summary>
/// Service interface for training the AI model from CSV data
/// </summary>
public interface IModelTrainingService
{
    /// <summary>
    /// Trains the triage prediction model from a CSV file
    /// </summary>
    /// <param name="csvStream">Stream containing the CSV data</param>
    /// <param name="progressCallback">Optional callback for training progress updates</param>
    /// <returns>Training result with success status and message</returns>
    Task<TrainingResult> TrainFromCsvAsync(Stream csvStream, Action<string>? progressCallback = null);
    
    /// <summary>
    /// Gets information about the currently loaded model
    /// </summary>
    ModelInfo GetModelInfo();
    
    /// <summary>
    /// Checks if a trained model exists
    /// </summary>
    bool HasTrainedModel { get; }
}

/// <summary>
/// Result of a training operation
/// </summary>
public class TrainingResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int RecordsProcessed { get; set; }
    public double? Accuracy { get; set; }
    public TimeSpan TrainingDuration { get; set; }
}

/// <summary>
/// Information about the current model
/// </summary>
public class ModelInfo
{
    public bool IsLoaded { get; set; }
    public DateTime? TrainedDate { get; set; }
    public int? TrainingRecords { get; set; }
    public string ModelPath { get; set; } = string.Empty;
}
