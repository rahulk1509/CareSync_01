using HospitalTriageAI.Models;
using HospitalTriageAI.Models.Enums;

namespace HospitalTriageAI.Services;

/// <summary>
/// Service for analyzing patient data and recommending appropriate hospital departments
/// </summary>
public interface IDepartmentAnalysisService
{
    /// <summary>
    /// Analyzes patient vitals and symptoms to recommend the most appropriate department
    /// </summary>
    /// <param name="assessment">The triage assessment containing vitals and symptoms</param>
    /// <param name="patient">The patient being assessed</param>
    /// <returns>Department analysis result with recommendation and explanation</returns>
    Task<DepartmentAnalysisResult> AnalyzeAsync(TriageAssessment assessment, Patient patient);
    
    /// <summary>
    /// Saves department prediction to the database
    /// </summary>
    /// <param name="prediction">The prediction to save</param>
    /// <returns>The saved prediction with generated ID</returns>
    Task<DepartmentPrediction> SavePredictionAsync(DepartmentPrediction prediction);
    
    /// <summary>
    /// Gets department prediction history for a patient
    /// </summary>
    /// <param name="patientId">The patient ID</param>
    /// <returns>List of past predictions</returns>
    Task<List<DepartmentPrediction>> GetPatientPredictionsAsync(int patientId);
    
    /// <summary>
    /// Gets the latest department prediction for a patient
    /// </summary>
    /// <param name="patientId">The patient ID</param>
    /// <returns>The most recent prediction or null</returns>
    Task<DepartmentPrediction?> GetLatestPredictionAsync(int patientId);
    
    /// <summary>
    /// Gets all predictions with optional filtering
    /// </summary>
    /// <param name="department">Optional department filter</param>
    /// <param name="limit">Maximum number of results</param>
    /// <returns>List of predictions</returns>
    Task<List<DepartmentPrediction>> GetPredictionsAsync(Department? department = null, int limit = 50);
    
    /// <summary>
    /// Gets department distribution statistics
    /// </summary>
    /// <returns>Dictionary of department to patient count</returns>
    Task<Dictionary<Department, int>> GetDepartmentDistributionAsync();
}
