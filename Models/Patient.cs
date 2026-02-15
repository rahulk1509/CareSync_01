using HospitalTriageAI.Models.Enums;

namespace HospitalTriageAI.Models;

/// <summary>
/// Patient entity with basic information
/// </summary>
public class Patient
{
    public int Id { get; set; }
    
    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    public string FullName => $"{FirstName} {LastName}";
    
    public DateTime DateOfBirth { get; set; }
    
    public int Age => (int)((DateTime.Now - DateOfBirth).TotalDays / 365.25);
    
    public string Gender { get; set; } = string.Empty;
    
    public string? PhoneNumber { get; set; }
    
    public string? Email { get; set; }
    
    public string? MedicalRecordNumber { get; set; }
    
    public string? ChiefComplaint { get; set; }
    
    public string? MedicalHistory { get; set; }
    
    public string? Allergies { get; set; }
    
    public string? CurrentMedications { get; set; }
    
    public TriageLevel CurrentTriageLevel { get; set; } = TriageLevel.Unassessed;
    
    public PatientStatus Status { get; set; } = PatientStatus.Waiting;
    
    public string? RecommendedDepartment { get; set; }
    
    public float? RiskPercentage { get; set; }
    
    // Previous EMR Report
    public byte[]? PreviousEmrReport { get; set; }
    
    public string? PreviousEmrFileName { get; set; }
    
    public DateTime? EmrUploadedAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime? LastUpdated { get; set; }
    
    // Navigation properties
    public List<TriageAssessment> Assessments { get; set; } = new();
    public List<PatientAssignment> Assignments { get; set; } = new();
}

/// <summary>
/// Patient queue status
/// </summary>
public enum PatientStatus
{
    Waiting = 0,
    Assigned = 1,
    InProgress = 2,
    Completed = 3,
    Discharged = 4
}
