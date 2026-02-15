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
    
    public async Task<Doctor> CreateDoctorAsync(Doctor doctor)
    {
        // Ensure new doctor starts with zero workload
        doctor.CurrentPatientCount = 0;
        doctor.CreatedAt = DateTime.Now;
        
        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();
        return doctor;
    }
    
    public async Task<Doctor?> UpdateDoctorAsync(Doctor doctor)
    {
        var existing = await _context.Doctors.FindAsync(doctor.Id);
        if (existing == null) return null;
        
        existing.Name = doctor.Name;
        existing.Department = doctor.Department;
        existing.Specialization = doctor.Specialization;
        existing.Email = doctor.Email;
        existing.Phone = doctor.Phone;
        existing.MaxPatientCapacity = doctor.MaxPatientCapacity;
        existing.IsAvailable = doctor.IsAvailable;
        existing.YearsOfExperience = doctor.YearsOfExperience;
        existing.Qualification = doctor.Qualification;
        
        await _context.SaveChangesAsync();
        return existing;
    }
    
    public async Task<bool> DeleteDoctorAsync(int id)
    {
        var doctor = await _context.Doctors.FindAsync(id);
        if (doctor == null) return false;
        
        _context.Doctors.Remove(doctor);
        await _context.SaveChangesAsync();
        return true;
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
    
    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        return await _context.Doctors
            .AnyAsync(d => d.Email != null && 
                          d.Email.ToLower() == email.ToLower() && 
                          (excludeId == null || d.Id != excludeId));
    }
}
