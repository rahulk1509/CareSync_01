using HospitalTriageAI.Models;
using HospitalTriageAI.Models.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestColors = QuestPDF.Helpers.Colors;
using QuestContainer = QuestPDF.Infrastructure.IContainer;

namespace HospitalTriageAI.Services;

public class PdfReportService : IPdfReportService
{
    public PdfReportService()
    {
        // Configure QuestPDF license (Community license for open source)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> GeneratePatientReportAsync(Patient patient, Doctor? assignedDoctor = null)
    {
        var document = CreateDocument(patient, assignedDoctor);
        var bytes = document.GeneratePdf();
        return Task.FromResult(bytes);
    }

    public async Task<string> SaveReportToFileAsync(Patient patient, Doctor? assignedDoctor = null)
    {
        var pdfBytes = await GeneratePatientReportAsync(patient, assignedDoctor);
        
        var fileName = $"TriageReport_{patient.FullName.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        var filePath = Path.Combine(FileSystem.AppDataDirectory, "Reports", fileName);
        
        // Ensure directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        
        await File.WriteAllBytesAsync(filePath, pdfBytes);
        return filePath;
    }

    private Document CreateDocument(Patient patient, Doctor? assignedDoctor)
    {
        // Get latest assessment if available
        var assessment = patient.Assessments?.OrderByDescending(a => a.AssessedAt).FirstOrDefault();
        
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(c => ComposeHeader(c, patient));
                page.Content().Element(c => ComposeContent(c, patient, assessment, assignedDoctor));
                page.Footer().Element(c => ComposeFooter(c));
            });
        });
    }

    private void ComposeHeader(QuestContainer container, Patient patient)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("HOSPITAL TRIAGE AI")
                        .Bold().FontSize(20).FontColor(QuestColors.Teal.Medium);
                    col.Item().Text("Patient Assessment Report")
                        .FontSize(12).FontColor(QuestColors.Grey.Medium);
                });

                row.ConstantItem(100).Column(col =>
                {
                    col.Item().AlignRight().Text($"Report #{patient.Id:D6}")
                        .FontSize(10).FontColor(QuestColors.Grey.Medium);
                    col.Item().AlignRight().Text(DateTime.Now.ToString("MMM dd, yyyy"))
                        .FontSize(10);
                });
            });

            column.Item().PaddingTop(10).LineHorizontal(2).LineColor(QuestColors.Teal.Medium);

            // Risk Banner
            column.Item().PaddingTop(15).Row(row =>
            {
                var (bgColor, textColor, label) = GetRiskColors(patient.CurrentTriageLevel);
                
                row.RelativeItem().Background(bgColor).Padding(10).Row(r =>
                {
                    r.RelativeItem().Column(c =>
                    {
                        c.Item().Text("AI RISK ASSESSMENT").Bold().FontSize(10).FontColor(textColor);
                        c.Item().Text(label).Bold().FontSize(16).FontColor(textColor);
                    });
                    r.ConstantItem(80).AlignRight().AlignMiddle()
                        .Text($"{(patient.RiskPercentage ?? 0) * 100:F0}%").Bold().FontSize(24).FontColor(textColor);
                });
            });
        });
    }

    private void ComposeContent(QuestContainer container, Patient patient, TriageAssessment? assessment, Doctor? assignedDoctor)
    {
        container.PaddingTop(20).Column(column =>
        {
            // Patient Information Section
            column.Item().Element(c => ComposeSection(c, "Patient Information", content =>
            {
                content.Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        ComposeInfoRow(col, "Full Name", patient.FullName);
                        ComposeInfoRow(col, "Age", $"{patient.Age} years");
                        ComposeInfoRow(col, "Gender", patient.Gender);
                    });
                    row.RelativeItem().Column(col =>
                    {
                        ComposeInfoRow(col, "Email", patient.Email ?? "N/A");
                        ComposeInfoRow(col, "Phone", patient.PhoneNumber ?? "N/A");
                        ComposeInfoRow(col, "Registration", patient.CreatedAt.ToString("MMM dd, yyyy HH:mm"));
                    });
                });
            }));

            // Vital Signs Section (if assessment exists)
            if (assessment != null)
            {
                column.Item().PaddingTop(15).Element(c => ComposeSection(c, "Vital Signs", content =>
                {
                    content.Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(QuestColors.Grey.Lighten3).Padding(5).Text("Measurement").Bold().FontSize(10);
                            header.Cell().Background(QuestColors.Grey.Lighten3).Padding(5).Text("Value").Bold().FontSize(10);
                            header.Cell().Background(QuestColors.Grey.Lighten3).Padding(5).Text("Measurement").Bold().FontSize(10);
                            header.Cell().Background(QuestColors.Grey.Lighten3).Padding(5).Text("Value").Bold().FontSize(10);
                        });

                        table.Cell().Padding(5).Text("Heart Rate").FontSize(10);
                        table.Cell().Padding(5).Text($"{assessment.HeartRate} bpm").FontSize(10);
                        table.Cell().Padding(5).Text("Blood Pressure").FontSize(10);
                        table.Cell().Padding(5).Text($"{assessment.BloodPressureSystolic}/{assessment.BloodPressureDiastolic} mmHg").FontSize(10);

                        table.Cell().Padding(5).Text("Temperature").FontSize(10);
                        table.Cell().Padding(5).Text($"{assessment.Temperature:F1}°C").FontSize(10);
                        table.Cell().Padding(5).Text("Oxygen Saturation").FontSize(10);
                        table.Cell().Padding(5).Text($"{assessment.OxygenSaturation}%").FontSize(10);

                        table.Cell().Padding(5).Text("Respiratory Rate").FontSize(10);
                        table.Cell().Padding(5).Text($"{assessment.RespiratoryRate} breaths/min").FontSize(10);
                        table.Cell().Padding(5).Text("Pain Level").FontSize(10);
                        table.Cell().Padding(5).Text($"{assessment.PainLevel}/10").FontSize(10);
                    });
                }));
            }

            // Symptoms Section
            column.Item().PaddingTop(15).Element(c => ComposeSection(c, "Reported Symptoms", content =>
            {
                content.Column(col =>
                {
                    col.Item().Text(patient.ChiefComplaint ?? "No symptoms reported")
                        .FontSize(11).Italic();
                    
                    if (assessment != null)
                    {
                        col.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Chest Pain:").Bold().FontSize(10);
                                c.Item().Text(GetSeverityLabel(assessment.ChestPain)).FontSize(10);
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Shortness of Breath:").Bold().FontSize(10);
                                c.Item().Text(GetSeverityLabel(assessment.ShortnessOfBreath)).FontSize(10);
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Fever:").Bold().FontSize(10);
                                c.Item().Text(GetSeverityLabel(assessment.Fever)).FontSize(10);
                            });
                        });
                    }
                });
            }));

            // AI Assessment Section
            column.Item().PaddingTop(15).Element(c => ComposeSection(c, "AI Triage Assessment", content =>
            {
                content.Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Triage Level:").Bold().FontSize(10);
                            c.Item().Text(GetTriageLevelLabel(patient.CurrentTriageLevel))
                                .FontSize(12).Bold();
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Risk Percentage:").Bold().FontSize(10);
                            c.Item().Text($"{(patient.RiskPercentage ?? 0) * 100:F1}%")
                                .FontSize(12).Bold();
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Recommended Department:").Bold().FontSize(10);
                            c.Item().Text(patient.RecommendedDepartment ?? "General Medicine")
                                .FontSize(12).Bold();
                        });
                    });

                    // AI Explanation
                    col.Item().PaddingTop(10).Background(QuestColors.Grey.Lighten4).Padding(10).Column(c =>
                    {
                        c.Item().Text("AI Analysis Notes:").Bold().FontSize(10);
                        c.Item().Text("Risk assessment calculated based on vital signs, symptom severity, and patient history. " +
                            "This is an AI-assisted preliminary assessment and should be verified by medical professionals.")
                            .FontSize(9).FontColor(QuestColors.Grey.Darken1);
                    });
                });
            }));

            // Assignment Section
            if (assignedDoctor != null)
            {
                column.Item().PaddingTop(15).Element(c => ComposeSection(c, "Doctor Assignment", content =>
                {
                    content.Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            ComposeInfoRow(col, "Assigned Doctor", assignedDoctor.Name);
                            ComposeInfoRow(col, "Department", assignedDoctor.Department.ToString());
                        });
                        row.RelativeItem().Column(col =>
                        {
                            ComposeInfoRow(col, "Specialization", assignedDoctor.Specialization ?? "N/A");
                            ComposeInfoRow(col, "Status", patient.Status.ToString());
                        });
                    });
                }));
            }
        });
    }

    private void ComposeSection(QuestContainer container, string title, Action<QuestContainer> content)
    {
        container.Column(column =>
        {
            column.Item().BorderBottom(1).BorderColor(QuestColors.Teal.Medium).PaddingBottom(5)
                .Text(title).Bold().FontSize(12).FontColor(QuestColors.Teal.Darken2);
            column.Item().PaddingTop(10).Element(content);
        });
    }

    private void ComposeInfoRow(ColumnDescriptor column, string label, string value)
    {
        column.Item().PaddingBottom(5).Row(row =>
        {
            row.ConstantItem(100).Text(label + ":").FontSize(10).FontColor(QuestColors.Grey.Darken1);
            row.RelativeItem().Text(value).FontSize(10).Bold();
        });
    }

    private void ComposeFooter(QuestContainer container)
    {
        container.Column(column =>
        {
            column.Item().LineHorizontal(1).LineColor(QuestColors.Grey.Lighten2);
            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span("Generated by Hospital Triage AI System").FontSize(8).FontColor(QuestColors.Grey.Medium);
                });
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Page ").FontSize(8).FontColor(QuestColors.Grey.Medium);
                    text.CurrentPageNumber().FontSize(8).FontColor(QuestColors.Grey.Medium);
                    text.Span(" of ").FontSize(8).FontColor(QuestColors.Grey.Medium);
                    text.TotalPages().FontSize(8).FontColor(QuestColors.Grey.Medium);
                });
            });
            column.Item().PaddingTop(5).AlignCenter()
                .Text("CONFIDENTIAL - This report contains protected health information")
                .FontSize(7).FontColor(QuestColors.Grey.Medium).Italic();
        });
    }

    private (string bgColor, string textColor, string label) GetRiskColors(TriageLevel level) => (int)level switch
    {
        1 => (QuestColors.Red.Lighten4, QuestColors.Red.Darken2, "EMERGENCY - Immediate Care Required"),
        2 => (QuestColors.Orange.Lighten4, QuestColors.Orange.Darken2, "URGENT - Priority Care Needed"),
        3 => (QuestColors.Blue.Lighten4, QuestColors.Blue.Darken2, "STANDARD - Timely Care"),
        4 => (QuestColors.Green.Lighten4, QuestColors.Green.Darken2, "LOW RISK - Routine Care"),
        _ => (QuestColors.Grey.Lighten4, QuestColors.Grey.Darken2, "UNASSESSED")
    };

    private string GetSeverityLabel(SymptomSeverity severity) => severity switch
    {
        SymptomSeverity.Mild => "Mild",
        SymptomSeverity.Moderate => "Moderate",
        SymptomSeverity.Severe => "Severe",
        SymptomSeverity.Critical => "Critical",
        _ => "None"
    };

    private string GetTriageLevelLabel(TriageLevel level) => (int)level switch
    {
        1 => "Emergency (Level 1)",
        2 => "Urgent (Level 2)",
        3 => "Standard (Level 3)",
        4 => "Non-Urgent (Level 4)",
        _ => "Unassessed"
    };
}
