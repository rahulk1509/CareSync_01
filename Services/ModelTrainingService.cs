using HospitalTriageAI.AI;

namespace HospitalTriageAI.Services;

/// <summary>
/// Service for training the AI model from CSV data
/// </summary>
public class ModelTrainingService : IModelTrainingService
{
    private readonly TriagePredictionEngine _predictionEngine;
    
    public ModelTrainingService(TriagePredictionEngine predictionEngine)
    {
        _predictionEngine = predictionEngine;
    }
    
    public bool HasTrainedModel => _predictionEngine.IsModelLoaded;
    
    public async Task<TrainingResult> TrainFromCsvAsync(Stream csvStream, Action<string>? progressCallback = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            var (success, message, accuracy) = await _predictionEngine.TrainModelAsync(csvStream, progressCallback);
            
            stopwatch.Stop();
            
            return new TrainingResult
            {
                Success = success,
                Message = message,
                RecordsProcessed = _predictionEngine.TrainingRecordCount ?? 0,
                Accuracy = accuracy,
                TrainingDuration = stopwatch.Elapsed
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new TrainingResult
            {
                Success = false,
                Message = $"Training failed: {ex.Message}",
                TrainingDuration = stopwatch.Elapsed
            };
        }
    }
    
    public ModelInfo GetModelInfo()
    {
        return new ModelInfo
        {
            IsLoaded = _predictionEngine.IsModelLoaded,
            TrainedDate = _predictionEngine.ModelTrainedDate,
            TrainingRecords = _predictionEngine.TrainingRecordCount,
            ModelPath = _predictionEngine.GetModelPath()
        };
    }
}
