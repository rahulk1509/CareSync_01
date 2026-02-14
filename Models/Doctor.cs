namespace HospitalTriageAI.Models;

/// <summary>
/// Doctor entity for availability management
/// </summary>
public class Doctor
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Department { get; set; } = string.Empty;
    
    public string? Specialization { get; set; }
    
    public bool IsAvailable { get; set; } = true;
    
    public int CurrentPatientCount { get; set; } = 0;
    
    public int MaxPatientCapacity { get; set; } = 5;
    
    public string? Email { get; set; }
    
    public string? Phone { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    // Navigation property
    public List<PatientAssignment> Assignments { get; set; } = new();
    
    /// <summary>
    /// Check if doctor can take more patients
    /// </summary>
    public bool CanAcceptPatients => IsAvailable && CurrentPatientCount < MaxPatientCapacity;
    
    /// <summary>
    /// Get availability status text
    /// </summary>
    public string AvailabilityStatus => IsAvailable 
        ? (CanAcceptPatients ? $"Available ({MaxPatientCapacity - CurrentPatientCount} slots)" : "At Capacity")
        : "Unavailable";
}
