using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using HospitalTriageAI.Data;
using HospitalTriageAI.Data.Repositories;
using HospitalTriageAI.Services;
using HospitalTriageAI.AI;

namespace HospitalTriageAI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Add Blazor WebView
        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // === DATABASE ===
        var dbPath = AppDbContext.GetDatabasePath();
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // === REPOSITORIES ===
        builder.Services.AddScoped<IPatientRepository, PatientRepository>();

        // === SERVICES ===
        builder.Services.AddScoped<IPatientService, PatientService>();
        builder.Services.AddScoped<ITriageService, TriageService>();
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddScoped<IDoctorService, DoctorService>();
        builder.Services.AddScoped<IAssignmentService, AssignmentService>();
        builder.Services.AddScoped<IPdfReportService, PdfReportService>();

        // === AI ===
        builder.Services.AddSingleton<TriagePredictionEngine>();
        builder.Services.AddSingleton<IModelTrainingService, ModelTrainingService>();

        var app = builder.Build();

        // Ensure database is created and migrated
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted(); // Reset for demo - remove in production
            db.Database.EnsureCreated();
        }

        return app;
    }
}
