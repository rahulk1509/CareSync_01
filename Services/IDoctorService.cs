using HospitalTriageAI.Models;

namespace HospitalTriageAI.Services;

/// <summary>
/// Doctor management service interface
/// </summary>
public interface IDoctorService
{
    /// <summary>
    /// Get all doctors
    /// </summary>
    Task<List<Doctor>> GetAllDoctorsAsync();
    
    /// <summary>
    /// Get available doctors
    /// </summary>
    Task<List<Doctor>> GetAvailableDoctorsAsync();
    
    /// <summary>
    /// Get doctors by department
    /// </summary>
    Task<List<Doctor>> GetDoctorsByDepartmentAsync(string department);
    
    /// <summary>
    /// Get doctor by ID
    /// </summary>
    Task<Doctor?> GetDoctorByIdAsync(int id);
    
    /// <summary>
    /// Toggle doctor availability
    /// </summary>
    Task<bool> ToggleAvailabilityAsync(int doctorId);
    
    /// <summary>
    /// Update doctor's patient count
    /// </summary>
    Task UpdatePatientCountAsync(int doctorId, int delta);
    
    /// <summary>
    /// Get doctor statistics
    /// </summary>
    Task<DoctorStats> GetStatsAsync();
}

/// <summary>
/// Doctor statistics DTO
/// </summary>
public class DoctorStats
{
    public int TotalDoctors { get; set; }
    public int AvailableDoctors { get; set; }
    public int TotalPatientLoad { get; set; }
    public int TotalCapacity { get; set; }
}
