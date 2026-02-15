using Microsoft.EntityFrameworkCore;
using HospitalTriageAI.Models;

namespace HospitalTriageAI.Data;

/// <summary>
/// SQLite database context for hospital triage system
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<TriageAssessment> Assessments => Set<TriageAssessment>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<PatientAssignment> PatientAssignments => Set<PatientAssignment>();
    public DbSet<DepartmentPrediction> DepartmentPredictions => Set<DepartmentPrediction>();
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Patient configuration
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(p => p.LastName).IsRequired().HasMaxLength(100);
            entity.HasMany(p => p.Assessments)
                  .WithOne(a => a.Patient)
                  .HasForeignKey(a => a.PatientId);
            entity.HasMany(p => p.Assignments)
                  .WithOne(a => a.Patient)
                  .HasForeignKey(a => a.PatientId);
        });
        
        // TriageAssessment configuration
        modelBuilder.Entity<TriageAssessment>(entity =>
        {
            entity.HasKey(a => a.Id);
        });
        
        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasOne(u => u.Patient)
                  .WithMany()
                  .HasForeignKey(u => u.PatientId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
        
        // Doctor configuration
        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).IsRequired().HasMaxLength(200);
            entity.Property(d => d.Department).IsRequired().HasMaxLength(100);
            entity.Property(d => d.Specialization).HasMaxLength(200);
            entity.Property(d => d.Email).HasMaxLength(256);
            entity.Property(d => d.Phone).HasMaxLength(20);
            entity.Property(d => d.Qualification).HasMaxLength(100);
            entity.HasIndex(d => d.Email).IsUnique();
            entity.HasMany(d => d.Assignments)
                  .WithOne(a => a.Doctor)
                  .HasForeignKey(a => a.DoctorId);
        });
        
        // PatientAssignment configuration
        modelBuilder.Entity<PatientAssignment>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasOne(a => a.Assessment)
                  .WithMany()
                  .HasForeignKey(a => a.AssessmentId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
        
        // DepartmentPrediction configuration
        modelBuilder.Entity<DepartmentPrediction>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.ClinicalExplanation).HasMaxLength(2000);
            entity.Property(p => p.KeyFindings).HasMaxLength(1000);
            entity.HasOne(p => p.Patient)
                  .WithMany()
                  .HasForeignKey(p => p.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(p => p.Assessment)
                  .WithMany()
                  .HasForeignKey(p => p.AssessmentId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
        
        // Seed Admin user (password: admin123)
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            Email = "admin@triageai.com",
            PasswordHash = HashPassword("admin123"),
            Role = UserRole.Admin,
            FullName = "System Administrator",
            CreatedAt = DateTime.Now,
            IsActive = true
        });
        
        // Seed Doctors
        modelBuilder.Entity<Doctor>().HasData(
            new Doctor
            {
                Id = 1,
                Name = "Dr. Sarah Johnson",
                Department = "Emergency",
                Specialization = "Emergency Medicine",
                IsAvailable = true,
                CurrentPatientCount = 0,
                MaxPatientCapacity = 5,
                Email = "sarah.johnson@hospital.com",
                CreatedAt = DateTime.Now
            },
            new Doctor
            {
                Id = 2,
                Name = "Dr. Michael Chen",
                Department = "Cardiology",
                Specialization = "Cardiovascular Disease",
                IsAvailable = true,
                CurrentPatientCount = 0,
                MaxPatientCapacity = 4,
                Email = "michael.chen@hospital.com",
                CreatedAt = DateTime.Now
            },
            new Doctor
            {
                Id = 3,
                Name = "Dr. Emily Rodriguez",
                Department = "General Medicine",
                Specialization = "Internal Medicine",
                IsAvailable = true,
                CurrentPatientCount = 0,
                MaxPatientCapacity = 6,
                Email = "emily.rodriguez@hospital.com",
                CreatedAt = DateTime.Now
            }
        );
    }
    
    /// <summary>
    /// Simple password hashing (use proper hashing in production)
    /// </summary>
    private static string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password + "TriageAI_Salt_2024");
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
    
    /// <summary>
    /// Gets the SQLite database path for MAUI
    /// </summary>
    public static string GetDatabasePath()
    {
        return Path.Combine(FileSystem.AppDataDirectory, "hospital_triage.db");
    }
}
