using System.ComponentModel.DataAnnotations;

namespace HospitalTriageAI.Models;

/// <summary>
/// Form model for creating/editing doctors with validation
/// </summary>
public class DoctorFormModel
{
    public int? Id { get; set; }
    
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 200 characters")]
    [Display(Name = "Full Name")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Specialization is required")]
    [StringLength(200, ErrorMessage = "Specialization cannot exceed 200 characters")]
    public string Specialization { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Department is required")]
    public string Department { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
    public string Email { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    [Display(Name = "Phone Number")]
    public string? Phone { get; set; }
    
    [Required(ErrorMessage = "Max patient capacity is required")]
    [Range(1, 50, ErrorMessage = "Max capacity must be between 1 and 50")]
    [Display(Name = "Max Patient Capacity")]
    public int MaxPatientCapacity { get; set; } = 5;
    
    [Display(Name = "Available")]
    public bool IsAvailable { get; set; } = true;
    
    [Range(0, 60, ErrorMessage = "Years of experience must be between 0 and 60")]
    [Display(Name = "Years of Experience")]
    public int YearsOfExperience { get; set; } = 0;
    
    [StringLength(100, ErrorMessage = "Qualification cannot exceed 100 characters")]
    [Display(Name = "Qualification (e.g., MBBS, MD)")]
    public string? Qualification { get; set; }
    
    /// <summary>
    /// Convert form model to Doctor entity
    /// </summary>
    public Doctor ToDoctor()
    {
        return new Doctor
        {
            Id = Id ?? 0,
            Name = Name,
            Specialization = Specialization,
            Department = Department,
            Email = Email,
            Phone = Phone,
            MaxPatientCapacity = MaxPatientCapacity,
            IsAvailable = IsAvailable,
            YearsOfExperience = YearsOfExperience,
            Qualification = Qualification,
            CurrentPatientCount = 0
        };
    }
    
    /// <summary>
    /// Create form model from existing Doctor entity
    /// </summary>
    public static DoctorFormModel FromDoctor(Doctor doctor)
    {
        return new DoctorFormModel
        {
            Id = doctor.Id,
            Name = doctor.Name,
            Specialization = doctor.Specialization ?? string.Empty,
            Department = doctor.Department,
            Email = doctor.Email ?? string.Empty,
            Phone = doctor.Phone,
            MaxPatientCapacity = doctor.MaxPatientCapacity,
            IsAvailable = doctor.IsAvailable,
            YearsOfExperience = doctor.YearsOfExperience,
            Qualification = doctor.Qualification
        };
    }
    
    /// <summary>
    /// Reset form to defaults
    /// </summary>
    public void Reset()
    {
        Id = null;
        Name = string.Empty;
        Specialization = string.Empty;
        Department = string.Empty;
        Email = string.Empty;
        Phone = null;
        MaxPatientCapacity = 5;
        IsAvailable = true;
        YearsOfExperience = 0;
        Qualification = null;
    }
}
