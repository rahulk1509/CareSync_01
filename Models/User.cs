namespace HospitalTriageAI.Models;

/// <summary>
/// User entity for authentication
/// </summary>
public class User
{
    public int Id { get; set; }
    
    public string Email { get; set; } = string.Empty;
    
    public string PasswordHash { get; set; } = string.Empty;
    
    public UserRole Role { get; set; } = UserRole.Patient;
    
    public string? FullName { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime? LastLoginAt { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Link to patient record if role is Patient
    public int? PatientId { get; set; }
    public Patient? Patient { get; set; }
}

/// <summary>
/// User roles in the system
/// </summary>
public enum UserRole
{
    Patient = 0,
    Admin = 1,
    Doctor = 2
}
