using System.ComponentModel.DataAnnotations;

namespace HospitalTriageAI.Models;

/// <summary>
/// Doctor entity for availability management
/// </summary>
public class Doctor
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 200 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Department is required")]
    [StringLength(100)]
    public string Department { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Specialization is required")]
    [StringLength(200)]
    public string? Specialization { get; set; }
    
    public bool IsAvailable { get; set; } = true;
    
    public int CurrentPatientCount { get; set; } = 0;
    
    [Range(1, 50, ErrorMessage = "Max capacity must be between 1 and 50")]
    public int MaxPatientCapacity { get; set; } = 5;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(256)]
    public string? Email { get; set; }
    
    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20)]
    public string? Phone { get; set; }
    
    [Range(0, 60, ErrorMessage = "Years of experience must be between 0 and 60")]
    public int YearsOfExperience { get; set; } = 0;
    
    [StringLength(100)]
    public string? Qualification { get; set; }
    
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
