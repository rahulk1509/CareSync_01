using Microsoft.EntityFrameworkCore;
using HospitalTriageAI.Data;
using HospitalTriageAI.Models;

namespace HospitalTriageAI.Services;

/// <summary>
/// Patient assignment service implementation
/// </summary>
public class AssignmentService : IAssignmentService
{
    private readonly AppDbContext _context;
    private readonly IDoctorService _doctorService;
    
    public AssignmentService(AppDbContext context, IDoctorService doctorService)
    {
        _context = context;
        _doctorService = doctorService;
    }
    
    public async Task<PatientAssignment?> AssignPatientAsync(int patientId, int doctorId, string? assignedBy = null)
    {
        var patient = await _context.Patients
            .Include(p => p.Assessments.OrderByDescending(a => a.AssessedAt).Take(1))
            .FirstOrDefaultAsync(p => p.Id == patientId);
        
        var doctor = await _context.Doctors.FindAsync(doctorId);
        
        if (patient == null || doctor == null) return null;
        if (!doctor.CanAcceptPatients) return null;
        
        var latestAssessment = patient.Assessments.FirstOrDefault();
        
        var assignment = new PatientAssignment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            AssessmentId = latestAssessment?.Id,
            Status = AssignmentStatus.Assigned,
            RecommendedDepartment = patient.RecommendedDepartment ?? doctor.Department,
            AssignedAt = DateTime.Now,
            AssignedBy = assignedBy
        };
        
        // Update patient status
        patient.Status = PatientStatus.Assigned;
        patient.LastUpdated = DateTime.Now;
        
        // Update doctor patient count
        doctor.CurrentPatientCount++;
        
        _context.PatientAssignments.Add(assignment);
        await _context.SaveChangesAsync();
        
        return assignment;
    }
    
    public async Task<bool> UpdateStatusAsync(int assignmentId, AssignmentStatus status)
    {
        var assignment = await _context.PatientAssignments
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.Id == assignmentId);
        
        if (assignment == null) return false;
        
        assignment.Status = status;
        
        // Sync patient status
        if (assignment.Patient != null)
        {
            assignment.Patient.Status = status switch
            {
                AssignmentStatus.Assigned => PatientStatus.Assigned,
                AssignmentStatus.InProgress => PatientStatus.InProgress,
                AssignmentStatus.Completed => PatientStatus.Completed,
                _ => assignment.Patient.Status
            };
            assignment.Patient.LastUpdated = DateTime.Now;
        }
        
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<List<PatientAssignment>> GetActiveAssignmentsAsync()
    {
        return await _context.PatientAssignments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Assessment)
            .Where(a => a.Status != AssignmentStatus.Completed && a.Status != AssignmentStatus.Cancelled)
            .OrderByDescending(a => a.Patient!.RiskPercentage ?? 0)
            .ToListAsync();
    }
    
    public async Task<List<PatientAssignment>> GetDoctorAssignmentsAsync(int doctorId)
    {
        return await _context.PatientAssignments
            .Include(a => a.Patient)
            .Include(a => a.Assessment)
            .Where(a => a.DoctorId == doctorId && a.Status != AssignmentStatus.Completed)
            .OrderByDescending(a => a.AssignedAt)
            .ToListAsync();
    }
    
    public async Task<PatientAssignment?> GetPatientAssignmentAsync(int patientId)
    {
        return await _context.PatientAssignments
            .Include(a => a.Doctor)
            .Where(a => a.PatientId == patientId && a.Status != AssignmentStatus.Completed)
            .OrderByDescending(a => a.AssignedAt)
            .FirstOrDefaultAsync();
    }
    
    public async Task<bool> CompleteAssignmentAsync(int assignmentId)
    {
        var assignment = await _context.PatientAssignments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == assignmentId);
        
        if (assignment == null) return false;
        
        assignment.Status = AssignmentStatus.Completed;
        assignment.CompletedAt = DateTime.Now;
        
        if (assignment.Patient != null)
        {
            assignment.Patient.Status = PatientStatus.Completed;
            assignment.Patient.LastUpdated = DateTime.Now;
        }
        
        if (assignment.Doctor != null)
        {
            assignment.Doctor.CurrentPatientCount = Math.Max(0, assignment.Doctor.CurrentPatientCount - 1);
        }
        
        await _context.SaveChangesAsync();
        return true;
    }
}
