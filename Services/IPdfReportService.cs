using HospitalTriageAI.Models;

namespace HospitalTriageAI.Services;

public interface IPdfReportService
{
    Task<byte[]> GeneratePatientReportAsync(Patient patient, Doctor? assignedDoctor = null);
    Task<string> SaveReportToFileAsync(Patient patient, Doctor? assignedDoctor = null);
}
