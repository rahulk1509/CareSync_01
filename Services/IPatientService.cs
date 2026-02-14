using HospitalTriageAI.Models;
using HospitalTriageAI.Models.Enums;

namespace HospitalTriageAI.Services;

/// <summary>
/// Service interface for patient operations
/// </summary>
public interface IPatientService
{
    Task<List<Patient>> GetAllPatientsAsync();
    Task<Patient?> GetPatientAsync(int id);
    Task<Patient> CreatePatientAsync(Patient patient);
    Task UpdatePatientAsync(Patient patient);
    Task DeletePatientAsync(int id);
    Task<List<Patient>> SearchPatientsAsync(string searchTerm);
    
    /// <summary>
    /// Get patients sorted by risk for queue display
    /// </summary>
    Task<List<Patient>> GetPatientQueueAsync();
    
    /// <summary>
    /// Get high risk patients (Emergency or Urgent)
    /// </summary>
    Task<List<Patient>> GetHighRiskPatientsAsync();
    
    /// <summary>
    /// Get patient statistics for dashboard
    /// </summary>
    Task<PatientStats> GetStatsAsync();
}

/// <summary>
/// Patient statistics DTO
/// </summary>
public class PatientStats
{
    public int TotalPatients { get; set; }
    public int HighRiskCount { get; set; }
    public int MediumRiskCount { get; set; }
    public int LowRiskCount { get; set; }
    public int WaitingCount { get; set; }
    public int AssignedCount { get; set; }
    public int CompletedToday { get; set; }
}
