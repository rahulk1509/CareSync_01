using Microsoft.EntityFrameworkCore;
using HospitalTriageAI.Data;
using HospitalTriageAI.Models;

namespace HospitalTriageAI.Services;

/// <summary>
/// Doctor management service implementation
/// </summary>
public class DoctorService : IDoctorService
{
    private readonly AppDbContext _context;
    
    public DoctorService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<Doctor>> GetAllDoctorsAsync()
    {
        return await _context.Doctors
            .Include(d => d.Assignments.Where(a => a.Status != AssignmentStatus.Completed))
            .OrderBy(d => d.Department)
            .ThenBy(d => d.Name)
            .ToListAsync();
    }
    
    public async Task<List<Doctor>> GetAvailableDoctorsAsync()
    {
        return await _context.Doctors
            .Where(d => d.IsAvailable && d.CurrentPatientCount < d.MaxPatientCapacity)
            .OrderBy(d => d.CurrentPatientCount)
            .ToListAsync();
    }
    
    public async Task<List<Doctor>> GetDoctorsByDepartmentAsync(string department)
    {
        return await _context.Doctors
            .Where(d => d.Department.ToLower() == department.ToLower())
            .OrderByDescending(d => d.IsAvailable)
            .ThenBy(d => d.CurrentPatientCount)
            .ToListAsync();
    }
    
    public async Task<Doctor?> GetDoctorByIdAsync(int id)
    {
        return await _context.Doctors
            .Include(d => d.Assignments)
                .ThenInclude(a => a.Patient)
            .FirstOrDefaultAsync(d => d.Id == id);
    }
    
    public async Task<bool> ToggleAvailabilityAsync(int doctorId)
    {
        var doctor = await _context.Doctors.FindAsync(doctorId);
        if (doctor == null) return false;
        
        doctor.IsAvailable = !doctor.IsAvailable;
        await _context.SaveChangesAsync();
        return doctor.IsAvailable;
    }
    
    public async Task UpdatePatientCountAsync(int doctorId, int delta)
    {
        var doctor = await _context.Doctors.FindAsync(doctorId);
        if (doctor == null) return;
        
        doctor.CurrentPatientCount = Math.Max(0, doctor.CurrentPatientCount + delta);
        await _context.SaveChangesAsync();
    }
    
    public async Task<DoctorStats> GetStatsAsync()
    {
        var doctors = await _context.Doctors.ToListAsync();
        return new DoctorStats
        {
            TotalDoctors = doctors.Count,
            AvailableDoctors = doctors.Count(d => d.IsAvailable && d.CanAcceptPatients),
            TotalPatientLoad = doctors.Sum(d => d.CurrentPatientCount),
            TotalCapacity = doctors.Sum(d => d.MaxPatientCapacity)
        };
    }
}
