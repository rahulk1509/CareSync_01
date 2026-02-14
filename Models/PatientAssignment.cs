namespace HospitalTriageAI.Models;

/// <summary>
/// Patient to Doctor assignment record
/// </summary>
public class PatientAssignment
{
    public int Id { get; set; }
    
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }
    
    public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; }
    
    public int? AssessmentId { get; set; }
    public TriageAssessment? Assessment { get; set; }
    
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Waiting;
    
    public string? RecommendedDepartment { get; set; }
    
    public string? Notes { get; set; }
    
    public DateTime AssignedAt { get; set; } = DateTime.Now;
    
    public DateTime? CompletedAt { get; set; }
    
    public string? AssignedBy { get; set; }
}

/// <summary>
/// Patient assignment status
/// </summary>
public enum AssignmentStatus
{
    Waiting = 0,
    Assigned = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}
