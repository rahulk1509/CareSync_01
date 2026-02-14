using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using HospitalTriageAI.Data;
using HospitalTriageAI.Models;
using HospitalTriageAI.Models.Enums;

namespace HospitalTriageAI.Services;

/// <summary>
/// Service for analyzing patient data and recommending appropriate hospital departments
/// using a rule-based weighted scoring system
/// </summary>
public class DepartmentAnalysisService : IDepartmentAnalysisService
{
    private readonly AppDbContext _context;
    
    public DepartmentAnalysisService(AppDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Analyzes patient vitals and symptoms to recommend the most appropriate department
    /// </summary>
    public async Task<DepartmentAnalysisResult> AnalyzeAsync(TriageAssessment assessment, Patient patient)
    {
        var scores = new List<DepartmentScore>();
        var keyFindings = new List<string>();
        
        // Calculate scores for each department
        scores.Add(CalculateEmergencyScore(assessment, keyFindings));
        scores.Add(CalculateCardiologyScore(assessment, keyFindings));
        scores.Add(CalculatePulmonologyScore(assessment, keyFindings));
        scores.Add(CalculateNeurologyScore(assessment, keyFindings));
        scores.Add(CalculateOrthopedicsScore(assessment, keyFindings));
        scores.Add(CalculateGeneralMedicineScore(assessment, keyFindings));
        
        // Sort by score descending
        scores = scores.OrderByDescending(s => s.Score).ToList();
        
        // Check for emergency priority conditions
        bool isEmergencyPriority = CheckEmergencyPriority(assessment);
        
        // Determine recommended department with priority rules
        var recommendedDept = DetermineRecommendedDepartment(scores, isEmergencyPriority);
        
        // Calculate confidence score
        int confidenceScore = CalculateConfidenceScore(scores, recommendedDept);
        
        // Generate clinical explanation
        string explanation = GenerateClinicalExplanation(assessment, patient, recommendedDept, scores, keyFindings);
        
        // Create and save prediction
        var prediction = new DepartmentPrediction
        {
            PatientId = patient.Id,
            AssessmentId = assessment.Id > 0 ? assessment.Id : null,
            RecommendedDepartment = recommendedDept,
            ConfidenceScore = confidenceScore,
            ClinicalExplanation = explanation,
            DepartmentScoresJson = JsonSerializer.Serialize(scores.Select(s => new { s.Department, s.Score })),
            IsEmergencyPriority = isEmergencyPriority,
            KeyFindings = string.Join("; ", keyFindings.Distinct()),
            PredictedAt = DateTime.Now
        };
        
        await SavePredictionAsync(prediction);
        
        // Update patient's recommended department
        patient.RecommendedDepartment = recommendedDept.ToString();
        patient.LastUpdated = DateTime.Now;
        _context.Patients.Update(patient);
        await _context.SaveChangesAsync();
        
        return new DepartmentAnalysisResult
        {
            RecommendedDepartment = recommendedDept,
            ConfidenceScore = confidenceScore,
            ClinicalExplanation = explanation,
            AllScores = scores,
            IsEmergencyPriority = isEmergencyPriority,
            KeyFindings = keyFindings.Distinct().ToList()
        };
    }
    
    #region Scoring Engine
    
    /// <summary>
    /// Calculates Emergency department score
    /// </summary>
    private DepartmentScore CalculateEmergencyScore(TriageAssessment assessment, List<string> keyFindings)
    {
        var score = new DepartmentScore { Department = Department.Emergency };
        
        // Critical O2 Saturation < 90
        if (assessment.OxygenSaturation < 90)
        {
            score.Score += 50;
            score.ContributingFactors.Add($"Critical O2 saturation ({assessment.OxygenSaturation:F0}%)");
            keyFindings.Add($"Critically low oxygen saturation: {assessment.OxygenSaturation:F0}%");
        }
        
        // Severe bleeding
        if (assessment.Bleeding == SymptomSeverity.Severe)
        {
            score.Score += 50;
            score.ContributingFactors.Add("Severe bleeding present");
            keyFindings.Add("Severe bleeding requiring immediate attention");
        }
        
        // Severe altered consciousness
        if (assessment.AlteredConsciousness == SymptomSeverity.Severe)
        {
            score.Score += 50;
            score.ContributingFactors.Add("Severe altered consciousness");
            keyFindings.Add("Severely altered level of consciousness");
        }
        
        // Critical vitals combination
        if (assessment.HeartRate > 130 || assessment.HeartRate < 50)
        {
            score.Score += 30;
            score.ContributingFactors.Add($"Critical heart rate ({assessment.HeartRate:F0} bpm)");
        }
        
        if (assessment.BloodPressureSystolic > 180 || assessment.BloodPressureSystolic < 80)
        {
            score.Score += 30;
            score.ContributingFactors.Add($"Critical blood pressure ({assessment.BloodPressureSystolic:F0}/{assessment.BloodPressureDiastolic:F0} mmHg)");
        }
        
        // Severe chest pain with other critical symptoms
        if (assessment.ChestPain == SymptomSeverity.Severe && assessment.ShortnessOfBreath >= SymptomSeverity.Moderate)
        {
            score.Score += 40;
            score.ContributingFactors.Add("Severe chest pain with respiratory distress");
            keyFindings.Add("Severe chest pain combined with breathing difficulty");
        }
        
        return score;
    }
    
    /// <summary>
    /// Calculates Cardiology department score
    /// </summary>
    private DepartmentScore CalculateCardiologyScore(TriageAssessment assessment, List<string> keyFindings)
    {
        var score = new DepartmentScore { Department = Department.Cardiology };
        
        // Chest pain ≥ Moderate
        if (assessment.ChestPain >= SymptomSeverity.Moderate)
        {
            score.Score += 40;
            score.ContributingFactors.Add($"{assessment.ChestPain} chest pain");
            keyFindings.Add($"{assessment.ChestPain} chest pain present");
        }
        
        // Heart rate > 110
        if (assessment.HeartRate > 110)
        {
            score.Score += 20;
            score.ContributingFactors.Add($"Elevated heart rate ({assessment.HeartRate:F0} bpm)");
            keyFindings.Add($"Tachycardia: {assessment.HeartRate:F0} bpm");
        }
        
        // Heart rate < 55 (bradycardia)
        if (assessment.HeartRate < 55)
        {
            score.Score += 25;
            score.ContributingFactors.Add($"Low heart rate ({assessment.HeartRate:F0} bpm)");
            keyFindings.Add($"Bradycardia: {assessment.HeartRate:F0} bpm");
        }
        
        // Systolic BP > 160
        if (assessment.BloodPressureSystolic > 160)
        {
            score.Score += 20;
            score.ContributingFactors.Add($"Elevated systolic BP ({assessment.BloodPressureSystolic:F0} mmHg)");
            keyFindings.Add($"Hypertension: {assessment.BloodPressureSystolic:F0}/{assessment.BloodPressureDiastolic:F0} mmHg");
        }
        
        // Shortness of breath ≥ Moderate (cardiac-related)
        if (assessment.ShortnessOfBreath >= SymptomSeverity.Moderate && assessment.ChestPain >= SymptomSeverity.Mild)
        {
            score.Score += 20;
            score.ContributingFactors.Add($"Shortness of breath with chest discomfort");
        }
        
        // Irregular vital pattern suggesting cardiac issue
        if (assessment.ChestPain >= SymptomSeverity.Mild && assessment.HeartRate > 100 && assessment.BloodPressureSystolic > 140)
        {
            score.Score += 15;
            score.ContributingFactors.Add("Multiple cardiovascular indicators");
        }
        
        return score;
    }
    
    /// <summary>
    /// Calculates Pulmonology department score
    /// </summary>
    private DepartmentScore CalculatePulmonologyScore(TriageAssessment assessment, List<string> keyFindings)
    {
        var score = new DepartmentScore { Department = Department.Pulmonology };
        
        // Shortness of breath ≥ Moderate
        if (assessment.ShortnessOfBreath >= SymptomSeverity.Moderate)
        {
            score.Score += 40;
            score.ContributingFactors.Add($"{assessment.ShortnessOfBreath} shortness of breath");
            keyFindings.Add($"{assessment.ShortnessOfBreath} respiratory distress");
        }
        
        // O2 Saturation < 94
        if (assessment.OxygenSaturation < 94 && assessment.OxygenSaturation >= 90)
        {
            score.Score += 30;
            score.ContributingFactors.Add($"Low O2 saturation ({assessment.OxygenSaturation:F0}%)");
            keyFindings.Add($"Hypoxemia: SpO2 {assessment.OxygenSaturation:F0}%");
        }
        
        // Respiratory rate > 22
        if (assessment.RespiratoryRate > 22)
        {
            score.Score += 20;
            score.ContributingFactors.Add($"Elevated respiratory rate ({assessment.RespiratoryRate:F0}/min)");
            keyFindings.Add($"Tachypnea: {assessment.RespiratoryRate:F0} breaths/min");
        }
        
        // Fever with respiratory symptoms
        if (assessment.Fever >= SymptomSeverity.Moderate && assessment.ShortnessOfBreath >= SymptomSeverity.Mild)
        {
            score.Score += 15;
            score.ContributingFactors.Add("Fever with respiratory symptoms");
        }
        
        // High temperature with breathing issues
        if (assessment.Temperature > 38.5 && assessment.ShortnessOfBreath >= SymptomSeverity.Mild)
        {
            score.Score += 10;
            score.ContributingFactors.Add($"Elevated temperature ({assessment.Temperature:F1}°C) with respiratory involvement");
        }
        
        return score;
    }
    
    /// <summary>
    /// Calculates Neurology department score
    /// </summary>
    private DepartmentScore CalculateNeurologyScore(TriageAssessment assessment, List<string> keyFindings)
    {
        var score = new DepartmentScore { Department = Department.Neurology };
        
        // Altered consciousness ≥ Mild
        if (assessment.AlteredConsciousness >= SymptomSeverity.Mild)
        {
            score.Score += 50;
            score.ContributingFactors.Add($"{assessment.AlteredConsciousness} altered consciousness");
            keyFindings.Add($"{assessment.AlteredConsciousness} altered level of consciousness");
        }
        
        // Severe headache (pain > 7 without chest pain emphasis)
        if (assessment.PainLevel > 7 && assessment.ChestPain <= SymptomSeverity.Mild)
        {
            score.Score += 20;
            score.ContributingFactors.Add($"Severe pain level ({assessment.PainLevel}/10) - possible headache");
            keyFindings.Add($"Severe pain: {assessment.PainLevel}/10");
        }
        
        // Altered consciousness with elevated BP (stroke risk)
        if (assessment.AlteredConsciousness >= SymptomSeverity.Mild && assessment.BloodPressureSystolic > 160)
        {
            score.Score += 20;
            score.ContributingFactors.Add("Altered consciousness with hypertension - stroke risk");
            keyFindings.Add("Elevated stroke risk factors");
        }
        
        return score;
    }
    
    /// <summary>
    /// Calculates Orthopedics department score
    /// </summary>
    private DepartmentScore CalculateOrthopedicsScore(TriageAssessment assessment, List<string> keyFindings)
    {
        var score = new DepartmentScore { Department = Department.Orthopedics };
        
        // Severe pain without cardiac or respiratory symptoms
        bool noCardiacSymptoms = assessment.ChestPain <= SymptomSeverity.Mild && 
                                 assessment.ShortnessOfBreath <= SymptomSeverity.Mild;
        bool noNeurologicalSymptoms = assessment.AlteredConsciousness == SymptomSeverity.None;
        
        if (assessment.PainLevel > 7 && noCardiacSymptoms && noNeurologicalSymptoms)
        {
            score.Score += 40;
            score.ContributingFactors.Add($"Severe localized pain ({assessment.PainLevel}/10)");
            keyFindings.Add($"Severe isolated pain: {assessment.PainLevel}/10 - possible musculoskeletal");
        }
        
        // Moderate pain with normal vitals
        bool normalVitals = assessment.HeartRate >= 60 && assessment.HeartRate <= 100 &&
                          assessment.BloodPressureSystolic >= 90 && assessment.BloodPressureSystolic <= 140;
        
        if (assessment.PainLevel >= 5 && assessment.PainLevel <= 7 && noCardiacSymptoms && normalVitals)
        {
            score.Score += 25;
            score.ContributingFactors.Add($"Moderate pain with stable vitals");
        }
        
        // Mild bleeding (possible trauma) with pain
        if (assessment.Bleeding == SymptomSeverity.Mild && assessment.PainLevel >= 4)
        {
            score.Score += 15;
            score.ContributingFactors.Add("Minor bleeding with pain - possible trauma");
        }
        
        return score;
    }
    
    /// <summary>
    /// Calculates General Medicine department score
    /// </summary>
    private DepartmentScore CalculateGeneralMedicineScore(TriageAssessment assessment, List<string> keyFindings)
    {
        var score = new DepartmentScore { Department = Department.GeneralMedicine };
        
        // High fever
        if (assessment.Fever >= SymptomSeverity.Severe || assessment.Temperature > 39)
        {
            score.Score += 30;
            score.ContributingFactors.Add($"High fever ({assessment.Temperature:F1}°C)");
            keyFindings.Add($"High fever: {assessment.Temperature:F1}°C");
        }
        else if (assessment.Fever == SymptomSeverity.Moderate || (assessment.Temperature > 38 && assessment.Temperature <= 39))
        {
            score.Score += 20;
            score.ContributingFactors.Add($"Moderate fever ({assessment.Temperature:F1}°C)");
        }
        
        // Mild abnormal vitals without specific department indicators
        bool mildlyAbnormalVitals = (assessment.HeartRate > 100 && assessment.HeartRate <= 110) ||
                                   (assessment.BloodPressureSystolic > 140 && assessment.BloodPressureSystolic <= 160);
        
        bool noSpecificSymptoms = assessment.ChestPain <= SymptomSeverity.Mild &&
                                  assessment.ShortnessOfBreath <= SymptomSeverity.Mild &&
                                  assessment.AlteredConsciousness == SymptomSeverity.None;
        
        if (mildlyAbnormalVitals && noSpecificSymptoms)
        {
            score.Score += 20;
            score.ContributingFactors.Add("Mildly abnormal vitals requiring general evaluation");
        }
        
        // Low-grade symptoms
        if (assessment.PainLevel >= 1 && assessment.PainLevel <= 4 && noSpecificSymptoms)
        {
            score.Score += 15;
            score.ContributingFactors.Add($"Mild pain ({assessment.PainLevel}/10) for general assessment");
        }
        
        // Mild bleeding without trauma indicators
        if (assessment.Bleeding == SymptomSeverity.Mild && assessment.PainLevel < 4)
        {
            score.Score += 10;
            score.ContributingFactors.Add("Minor bleeding for evaluation");
        }
        
        // Default baseline for patients without specific indicators
        if (score.Score == 0)
        {
            score.Score = 10;
            score.ContributingFactors.Add("General evaluation recommended");
        }
        
        return score;
    }
    
    #endregion
    
    #region Priority and Confidence Logic
    
    /// <summary>
    /// Checks for emergency priority conditions
    /// </summary>
    private bool CheckEmergencyPriority(TriageAssessment assessment)
    {
        return assessment.OxygenSaturation < 90 ||
               assessment.Bleeding == SymptomSeverity.Severe ||
               assessment.AlteredConsciousness == SymptomSeverity.Severe ||
               (assessment.ChestPain == SymptomSeverity.Severe && assessment.ShortnessOfBreath >= SymptomSeverity.Moderate) ||
               assessment.HeartRate > 150 || assessment.HeartRate < 40 ||
               assessment.BloodPressureSystolic > 200 || assessment.BloodPressureSystolic < 70;
    }
    
    /// <summary>
    /// Determines recommended department based on scores and priority rules
    /// Priority: Emergency > Cardiology > Others
    /// </summary>
    private Department DetermineRecommendedDepartment(List<DepartmentScore> scores, bool isEmergencyPriority)
    {
        if (isEmergencyPriority)
        {
            return Department.Emergency;
        }
        
        var topScore = scores.First();
        var secondScore = scores.Count > 1 ? scores[1] : null;
        
        // If scores are close (within 10 points), apply priority rules
        if (secondScore != null && topScore.Score - secondScore.Score <= 10)
        {
            var topDepartments = scores.Where(s => s.Score >= topScore.Score - 10)
                                       .Select(s => s.Department)
                                       .ToList();
            
            // Priority order
            var priorityOrder = new[] { Department.Emergency, Department.Cardiology, Department.Pulmonology, 
                                       Department.Neurology, Department.Orthopedics, Department.GeneralMedicine };
            
            foreach (var dept in priorityOrder)
            {
                if (topDepartments.Contains(dept))
                {
                    return dept;
                }
            }
        }
        
        return topScore.Department;
    }
    
    /// <summary>
    /// Calculates confidence score based on scoring spread
    /// </summary>
    private int CalculateConfidenceScore(List<DepartmentScore> scores, Department recommended)
    {
        var topScore = scores.First(s => s.Department == recommended);
        var otherScores = scores.Where(s => s.Department != recommended).ToList();
        
        if (!otherScores.Any() || topScore.Score == 0)
        {
            return 50; // Baseline confidence
        }
        
        var secondHighest = otherScores.Max(s => s.Score);
        var scoreDifference = topScore.Score - secondHighest;
        
        // Confidence based on margin
        int confidence;
        if (scoreDifference >= 40)
            confidence = 95;
        else if (scoreDifference >= 30)
            confidence = 85;
        else if (scoreDifference >= 20)
            confidence = 75;
        else if (scoreDifference >= 10)
            confidence = 65;
        else
            confidence = 55;
        
        // Boost confidence if top score is high
        if (topScore.Score >= 80)
            confidence = Math.Min(confidence + 5, 99);
        
        // Reduce confidence if multiple departments have similar scores
        var closeScores = scores.Count(s => s.Score >= topScore.Score - 15 && s.Department != recommended);
        if (closeScores >= 2)
            confidence = Math.Max(confidence - 10, 40);
        
        return confidence;
    }
    
    #endregion
    
    #region Explanation Generator
    
    /// <summary>
    /// Generates a clinical explanation referencing actual input values
    /// </summary>
    private string GenerateClinicalExplanation(
        TriageAssessment assessment, 
        Patient patient, 
        Department recommended,
        List<DepartmentScore> scores,
        List<string> keyFindings)
    {
        var sb = new StringBuilder();
        var deptScore = scores.First(s => s.Department == recommended);
        
        // Opening statement with patient context
        sb.Append($"Patient {patient.FullName} presents with ");
        
        // Build symptom description
        var symptoms = new List<string>();
        
        if (assessment.ChestPain >= SymptomSeverity.Mild)
            symptoms.Add($"{assessment.ChestPain.ToString().ToLower()} chest pain");
        
        if (assessment.ShortnessOfBreath >= SymptomSeverity.Mild)
            symptoms.Add($"{assessment.ShortnessOfBreath.ToString().ToLower()} shortness of breath");
        
        if (assessment.AlteredConsciousness >= SymptomSeverity.Mild)
            symptoms.Add($"{assessment.AlteredConsciousness.ToString().ToLower()} altered consciousness");
        
        if (assessment.Bleeding >= SymptomSeverity.Mild)
            symptoms.Add($"{assessment.Bleeding.ToString().ToLower()} bleeding");
        
        if (assessment.Fever >= SymptomSeverity.Mild)
            symptoms.Add($"fever ({assessment.Temperature:F1}°C)");
        
        if (assessment.PainLevel > 3)
            symptoms.Add($"pain level {assessment.PainLevel}/10");
        
        if (symptoms.Any())
        {
            sb.Append(string.Join(", ", symptoms));
        }
        else
        {
            sb.Append("non-specific symptoms");
        }
        
        // Add vital signs context
        sb.Append(". ");
        var vitalFindings = new List<string>();
        
        if (assessment.HeartRate > 100)
            vitalFindings.Add($"elevated heart rate ({assessment.HeartRate:F0} bpm)");
        else if (assessment.HeartRate < 60)
            vitalFindings.Add($"low heart rate ({assessment.HeartRate:F0} bpm)");
        
        if (assessment.BloodPressureSystolic > 140 || assessment.BloodPressureDiastolic > 90)
            vitalFindings.Add($"elevated blood pressure ({assessment.BloodPressureSystolic:F0}/{assessment.BloodPressureDiastolic:F0} mmHg)");
        else if (assessment.BloodPressureSystolic < 90)
            vitalFindings.Add($"low blood pressure ({assessment.BloodPressureSystolic:F0}/{assessment.BloodPressureDiastolic:F0} mmHg)");
        
        if (assessment.OxygenSaturation < 95)
            vitalFindings.Add($"reduced oxygen saturation ({assessment.OxygenSaturation:F0}%)");
        
        if (assessment.RespiratoryRate > 20)
            vitalFindings.Add($"elevated respiratory rate ({assessment.RespiratoryRate:F0}/min)");
        
        if (vitalFindings.Any())
        {
            sb.Append($"Vital signs show {string.Join(" and ", vitalFindings)}. ");
        }
        
        // Clinical interpretation
        sb.Append(GetDepartmentInterpretation(recommended, deptScore));
        
        // Recommendation
        sb.Append($" Therefore, {recommended} is recommended for further evaluation");
        
        if (deptScore.Score >= 60)
            sb.Append(" with priority attention");
        
        sb.Append(".");
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Gets clinical interpretation based on department
    /// </summary>
    private string GetDepartmentInterpretation(Department department, DepartmentScore score)
    {
        return department switch
        {
            Department.Emergency => "These findings indicate a potentially life-threatening condition requiring immediate emergency intervention.",
            Department.Cardiology => "These findings suggest possible cardiovascular involvement and cardiac stress that warrants cardiological assessment.",
            Department.Pulmonology => "These findings indicate respiratory compromise requiring pulmonary evaluation and management.",
            Department.Neurology => "These findings suggest neurological involvement that requires specialized neurological assessment.",
            Department.Orthopedics => "The pain pattern and presentation suggest musculoskeletal involvement requiring orthopedic evaluation.",
            Department.GeneralMedicine => "These findings warrant general medical evaluation to determine the underlying cause.",
            _ => "Clinical evaluation is recommended."
        };
    }
    
    #endregion
    
    #region Database Operations
    
    public async Task<DepartmentPrediction> SavePredictionAsync(DepartmentPrediction prediction)
    {
        _context.DepartmentPredictions.Add(prediction);
        await _context.SaveChangesAsync();
        return prediction;
    }
    
    public async Task<List<DepartmentPrediction>> GetPatientPredictionsAsync(int patientId)
    {
        return await _context.DepartmentPredictions
            .Where(p => p.PatientId == patientId)
            .OrderByDescending(p => p.PredictedAt)
            .ToListAsync();
    }
    
    public async Task<DepartmentPrediction?> GetLatestPredictionAsync(int patientId)
    {
        return await _context.DepartmentPredictions
            .Where(p => p.PatientId == patientId)
            .OrderByDescending(p => p.PredictedAt)
            .FirstOrDefaultAsync();
    }
    
    public async Task<List<DepartmentPrediction>> GetPredictionsAsync(Department? department = null, int limit = 50)
    {
        var query = _context.DepartmentPredictions
            .Include(p => p.Patient)
            .AsQueryable();
        
        if (department.HasValue)
        {
            query = query.Where(p => p.RecommendedDepartment == department.Value);
        }
        
        return await query
            .OrderByDescending(p => p.PredictedAt)
            .Take(limit)
            .ToListAsync();
    }
    
    public async Task<Dictionary<Department, int>> GetDepartmentDistributionAsync()
    {
        var predictions = await _context.DepartmentPredictions
            .GroupBy(p => p.RecommendedDepartment)
            .Select(g => new { Department = g.Key, Count = g.Count() })
            .ToListAsync();
        
        return predictions.ToDictionary(p => p.Department, p => p.Count);
    }
    
    #endregion
}
