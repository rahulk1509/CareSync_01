using HospitalTriageAI.Models;

namespace HospitalTriageAI.Services;

/// <summary>
/// Patient assignment service interface
/// </summary>
public interface IAssignmentService
{
    /// <summary>
    /// Assign patient to doctor
    /// </summary>
    Task<PatientAssignment?> AssignPatientAsync(int patientId, int doctorId, string? assignedBy = null);
    
    /// <summary>
    /// Update assignment status
    /// </summary>
    Task<bool> UpdateStatusAsync(int assignmentId, AssignmentStatus status);
    
    /// <summary>
    /// Get all active assignments
    /// </summary>
    Task<List<PatientAssignment>> GetActiveAssignmentsAsync();
    
    /// <summary>
    /// Get assignments for a doctor
    /// </summary>
    Task<List<PatientAssignment>> GetDoctorAssignmentsAsync(int doctorId);
    
    /// <summary>
    /// Get patient's current assignment
    /// </summary>
    Task<PatientAssignment?> GetPatientAssignmentAsync(int patientId);
    
    /// <summary>
    /// Complete an assignment
    /// </summary>
    Task<bool> CompleteAssignmentAsync(int assignmentId);
}
