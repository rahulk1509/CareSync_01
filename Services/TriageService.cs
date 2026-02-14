using Microsoft.EntityFrameworkCore;
using HospitalTriageAI.AI;
using HospitalTriageAI.Data;
using HospitalTriageAI.Models;
using HospitalTriageAI.Models.Enums;

namespace HospitalTriageAI.Services;

/// <summary>
/// Triage service implementation with AI integration
/// </summary>
public class TriageService : ITriageService
{
    private readonly AppDbContext _context;
    private readonly TriagePredictionEngine _predictionEngine;
    private readonly IDepartmentAnalysisService _departmentAnalysisService;
    
    public TriageService(
        AppDbContext context, 
        TriagePredictionEngine predictionEngine,
        IDepartmentAnalysisService departmentAnalysisService)
    {
        _context = context;
        _predictionEngine = predictionEngine;
        _departmentAnalysisService = departmentAnalysisService;
    }
    
    public async Task<RiskPrediction> AssessPatientAsync(Patient patient, TriageAssessment assessment)
    {
        // Get AI prediction
        var prediction = _predictionEngine.Predict(assessment, patient.Age);
        
        // Update assessment with AI results
        assessment.AiRiskScore = prediction.RiskScore;
        assessment.AssignedLevel = prediction.PredictedLevel;
        assessment.AssessedAt = DateTime.Now;
        
        // Update patient's current triage level
        patient.CurrentTriageLevel = prediction.PredictedLevel;
        patient.RiskPercentage = prediction.RiskScore;
        patient.LastUpdated = DateTime.Now;
        
        // Save assessment first to get ID
        _context.Assessments.Add(assessment);
        await _context.SaveChangesAsync();
        
        // Run department analysis
        try
        {
            var deptResult = await _departmentAnalysisService.AnalyzeAsync(assessment, patient);
            patient.RecommendedDepartment = deptResult.RecommendedDepartment.ToString();
        }
        catch
        {
            // Fallback to default department based on triage level
            patient.RecommendedDepartment = GetDefaultDepartment(prediction.PredictedLevel);
        }
        
        // Save patient updates
        _context.Patients.Update(patient);
        await _context.SaveChangesAsync();
        
        return prediction;
    }
    
    public async Task<List<TriageAssessment>> GetPatientAssessmentsAsync(int patientId)
    {
        return await _context.Assessments
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.AssessedAt)
            .ToListAsync();
    }
    
    public async Task SaveAssessmentAsync(TriageAssessment assessment)
    {
        _context.Assessments.Add(assessment);
        await _context.SaveChangesAsync();
    }
    
    public async Task<Dictionary<TriageLevel, List<Patient>>> GetPatientsByTriageLevelAsync()
    {
        var patients = await _context.Patients
            .Where(p => p.CurrentTriageLevel != TriageLevel.Unassessed)
            .ToListAsync();
        
        return patients
            .GroupBy(p => p.CurrentTriageLevel)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
    
    /// <summary>
    /// Gets default department based on triage level (fallback)
    /// </summary>
    private string GetDefaultDepartment(TriageLevel level) => level switch
    {
        TriageLevel.Emergency => Department.Emergency.ToString(),
        TriageLevel.Urgent => Department.Emergency.ToString(),
        TriageLevel.Standard => Department.GeneralMedicine.ToString(),
        TriageLevel.NonUrgent => Department.GeneralMedicine.ToString(),
        _ => Department.GeneralMedicine.ToString()
    };
}
