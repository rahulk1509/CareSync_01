using Microsoft.EntityFrameworkCore;
using HospitalTriageAI.Data;
using HospitalTriageAI.Data.Repositories;
using HospitalTriageAI.Models;
using HospitalTriageAI.Models.Enums;

namespace HospitalTriageAI.Services;

/// <summary>
/// Patient service implementation
/// </summary>
public class PatientService : IPatientService
{
    private readonly IPatientRepository _repository;
    private readonly AppDbContext _context;
    
    public PatientService(IPatientRepository repository, AppDbContext context)
    {
        _repository = repository;
        _context = context;
    }
    
    public async Task<List<Patient>> GetAllPatientsAsync()
    {
        return await _repository.GetAllAsync();
    }
    
    public async Task<Patient?> GetPatientAsync(int id)
    {
        return await _repository.GetByIdWithAssessmentsAsync(id);
    }
    
    public async Task<Patient> CreatePatientAsync(Patient patient)
    {
        // Generate medical record number if not provided
        if (string.IsNullOrEmpty(patient.MedicalRecordNumber))
        {
            patient.MedicalRecordNumber = GenerateMRN();
        }
        
        return await _repository.AddAsync(patient);
    }
    
    public async Task UpdatePatientAsync(Patient patient)
    {
        await _repository.UpdateAsync(patient);
    }
    
    public async Task DeletePatientAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }
    
    public async Task<List<Patient>> SearchPatientsAsync(string searchTerm)
    {
        return await _repository.SearchAsync(searchTerm);
    }
    
    public async Task<List<Patient>> GetPatientQueueAsync()
    {
        return await _context.Patients
            .Include(p => p.Assessments.OrderByDescending(a => a.AssessedAt).Take(1))
            .Include(p => p.Assignments.Where(a => a.Status != AssignmentStatus.Completed))
                .ThenInclude(a => a.Doctor)
            .Where(p => p.CurrentTriageLevel != TriageLevel.Unassessed)
            .Where(p => p.Status != PatientStatus.Completed && p.Status != PatientStatus.Discharged)
            .OrderBy(p => p.CurrentTriageLevel)
            .ThenByDescending(p => p.RiskPercentage ?? 0)
            .ThenBy(p => p.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<List<Patient>> GetHighRiskPatientsAsync()
    {
        return await _context.Patients
            .Include(p => p.Assignments.Where(a => a.Status != AssignmentStatus.Completed))
                .ThenInclude(a => a.Doctor)
            .Where(p => p.CurrentTriageLevel == TriageLevel.Emergency || p.CurrentTriageLevel == TriageLevel.Urgent)
            .Where(p => p.Status != PatientStatus.Completed && p.Status != PatientStatus.Discharged)
            .OrderBy(p => p.CurrentTriageLevel)
            .ThenByDescending(p => p.RiskPercentage ?? 0)
            .ToListAsync();
    }
    
    public async Task<PatientStats> GetStatsAsync()
    {
        var patients = await _context.Patients
            .Where(p => p.CurrentTriageLevel != TriageLevel.Unassessed)
            .ToListAsync();
        
        var today = DateTime.Today;
        
        return new PatientStats
        {
            TotalPatients = patients.Count,
            HighRiskCount = patients.Count(p => p.CurrentTriageLevel == TriageLevel.Emergency || p.CurrentTriageLevel == TriageLevel.Urgent),
            MediumRiskCount = patients.Count(p => p.CurrentTriageLevel == TriageLevel.Standard),
            LowRiskCount = patients.Count(p => p.CurrentTriageLevel == TriageLevel.NonUrgent),
            WaitingCount = patients.Count(p => p.Status == PatientStatus.Waiting),
            AssignedCount = patients.Count(p => p.Status == PatientStatus.Assigned || p.Status == PatientStatus.InProgress),
            CompletedToday = patients.Count(p => p.Status == PatientStatus.Completed && p.LastUpdated?.Date == today)
        };
    }
    
    private string GenerateMRN()
    {
        return $"MRN{DateTime.Now:yyyyMMdd}{Random.Shared.Next(1000, 9999)}";
    }
}
