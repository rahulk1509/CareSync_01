# CareSync - AI-Powered Hospital Triage Platform

<div align="center">

![CareSync Logo](wwwroot/logo.png)

**Intelligent Patient Triage & Healthcare Management System**

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![MAUI](https://img.shields.io/badge/MAUI-Blazor-512BD4?style=flat-square&logo=dotnet)](https://learn.microsoft.com/en-us/dotnet/maui/)
[![ML.NET](https://img.shields.io/badge/ML.NET-3.0-green?style=flat-square)](https://dotnet.microsoft.com/apps/machinelearning-ai/ml-dotnet)
[![License](https://img.shields.io/badge/License-MIT-blue?style=flat-square)](LICENSE)

</div>

---

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [User Roles](#user-roles)
- [AI/ML Features](#aiml-features)
- [Key Components](#key-components)
- [Screenshots](#screenshots)
- [Contributing](#contributing)
- [License](#license)

---

## Overview

**CareSync** is a cross-platform healthcare management application that leverages artificial intelligence to streamline emergency department triage processes. Built with .NET MAUI Blazor, the platform provides intelligent patient risk assessment, automated department routing, and comprehensive healthcare workflow management.

The system uses machine learning models trained on patient data to predict triage levels, enabling faster and more consistent patient prioritization in busy hospital environments.

### Key Benefits

- **Faster Triage Decisions**: AI-powered risk assessment reduces wait times
- **Consistent Prioritization**: ML models ensure standardized triage across shifts
- **Cross-Platform**: Runs on Windows, macOS, iOS, and Android
- **Bias Monitoring**: Built-in fairness analysis to detect demographic disparities
- **Comprehensive Workflow**: From patient intake to doctor assignment

---

## Features

### 🏥 Patient Management
- **Patient Intake**: Complete registration with demographics, medical history, allergies, and current medications
- **EMR Upload**: Support for uploading previous Electronic Medical Records
- **Patient Portal**: Self-service portal for patients to view their status and reports
- **Real-time Status Tracking**: Monitor patient journey from waiting to discharge

### 🤖 AI-Powered Triage Assessment
- **Risk Level Prediction**: ML.NET-based model predicts patient urgency (Emergency, Urgent, Standard, Non-Urgent)
- **Confidence Scoring**: Each prediction includes a confidence percentage
- **Risk Factor Analysis**: Identifies key factors influencing the triage decision
- **Smart Recommendations**: AI-generated recommendations for healthcare staff

### 👨‍⚕️ Doctor Management
- **Doctor Registration**: Add doctors with specializations, qualifications, and contact info
- **Availability Tracking**: Real-time availability status and patient capacity
- **Department Assignment**: Organize doctors by department specialization
- **Workload Balancing**: Track current patient count vs. maximum capacity

### 📊 Department Analysis
- **Smart Routing**: AI recommends the most appropriate department based on symptoms
- **Supported Departments**:
  - Emergency
  - Cardiology
  - Pulmonology
  - Neurology
  - Orthopedics
  - Pediatrics
  - General Medicine
  - Surgery
  - ICU

### 📈 Analytics & Reporting
- **Admin Dashboard**: Overview of patient distribution by triage level
- **PDF Reports**: Generate comprehensive patient reports with triage details
- **Department Statistics**: Analyze patient distribution across departments

### 🔬 AI Model Management
- **Custom Model Training**: Upload CSV data to train personalized triage models
- **Model Versioning**: Track when models were trained and on how many records
- **Bias/Fairness Analysis**: Monitor model performance across demographics
  - Gender-based accuracy comparison
  - Age group distribution analysis
  - False positive/negative rate tracking
  - Overall fairness scoring

### 🔐 Authentication & Authorization
- **Role-Based Access Control**: Patient, Doctor, and Admin roles
- **Secure Login**: Email/password authentication with password hashing
- **User Registration**: Self-service signup for patients
- **Session Management**: Persistent login state

---

## Technology Stack

| Layer | Technology |
|-------|------------|
| **Framework** | .NET 10.0 MAUI Blazor Hybrid |
| **UI** | Blazor Components with Scoped CSS |
| **Database** | SQLite with Entity Framework Core |
| **AI/ML** | ML.NET 3.0 |
| **PDF Generation** | iTextSharp / Custom PDF Service |
| **Authentication** | Custom auth with BCrypt hashing |

### Supported Platforms

| Platform | Minimum Version |
|----------|-----------------|
| Windows | 10.0.17763.0 |
| Android | API 24 (Android 7.0) |
| iOS | 15.0 |
| macOS (Catalyst) | 15.0 |

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│                   (Blazor Components)                        │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐       │
│  │  Pages   │ │  Layout  │ │  Shared  │ │   CSS    │       │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘       │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     Service Layer                            │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐        │
│  │ AuthService  │ │TriageService │ │DoctorService │        │
│  └──────────────┘ └──────────────┘ └──────────────┘        │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐        │
│  │PatientService│ │AssignmentSvc │ │  PdfService  │        │
│  └──────────────┘ └──────────────┘ └──────────────┘        │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      AI/ML Layer                             │
│  ┌────────────────────┐ ┌────────────────────┐              │
│  │TriagePrediction    │ │ ModelTraining      │              │
│  │     Engine         │ │    Service         │              │
│  └────────────────────┘ └────────────────────┘              │
│  ┌────────────────────┐ ┌────────────────────┐              │
│  │ Department         │ │  BiasAnalysis      │              │
│  │ AnalysisService    │ │    Result          │              │
│  └────────────────────┘ └────────────────────┘              │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      Data Layer                              │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐        │
│  │ AppDbContext │ │ Repositories │ │   SQLite     │        │
│  └──────────────┘ └──────────────┘ └──────────────┘        │
└─────────────────────────────────────────────────────────────┘
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2022 (17.8+) with MAUI workload, or
- VS Code with .NET MAUI extension
- Platform-specific requirements:
  - **Windows**: Windows 10/11 SDK
  - **Android**: Android SDK, Java JDK
  - **iOS/macOS**: Xcode 15+ (Mac only)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/CareSync.git
   cd CareSync
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the application**
   ```bash
   dotnet build
   ```

4. **Run on Windows**
   ```bash
   dotnet run --framework net10.0-windows10.0.19041.0
   ```

   Or run on Android:
   ```bash
   dotnet run --framework net10.0-android
   ```

### First-Time Setup

1. Launch the application
2. The database will be automatically created on first run
3. Register as a new user (default role: Patient)
4. For admin access, manually update the user role in the database or use seed data

---

## Project Structure

```
CareSync/
├── AI/                          # AI/ML components
│   ├── CsvTrainingInput.cs      # CSV data model for training
│   ├── ModelInput.cs            # ML model input schema
│   ├── ModelOutput.cs           # ML model output schema
│   ├── TriagePredictionEngine.cs # ML.NET prediction wrapper
│   └── Models/                  # Trained model storage
│
├── Components/                  # Blazor UI components
│   ├── Layout/                  # Layout components
│   │   ├── MainLayout.razor     # Main app layout
│   │   ├── AdminSidebar.razor   # Admin navigation
│   │   ├── PatientSidebar.razor # Patient navigation
│   │   └── NavMenu.razor        # Navigation menu
│   │
│   ├── Pages/                   # Page components
│   │   ├── Home.razor           # Landing page
│   │   ├── Login.razor          # User login
│   │   ├── Signup.razor         # User registration
│   │   ├── PatientIntake.razor  # Patient registration
│   │   ├── TriageAssessment.razor # Triage form
│   │   ├── AdminDashboard.razor # Admin overview
│   │   ├── DoctorManagement.razor # Doctor CRUD
│   │   ├── AIModelManagement.razor # AI model training
│   │   ├── PatientPortal.razor  # Patient self-service
│   │   ├── PatientReport.razor  # Patient details
│   │   └── Reports.razor        # Analytics
│   │
│   └── Shared/                  # Reusable components
│       ├── ProfileDropdown.razor # User profile menu
│       ├── RiskBadge.razor      # Triage level badge
│       └── RoleGuard.razor      # Authorization wrapper
│
├── Data/                        # Data access layer
│   ├── AppDbContext.cs          # EF Core DbContext
│   └── Repositories/            # Repository pattern
│
├── Models/                      # Domain models
│   ├── Patient.cs               # Patient entity
│   ├── Doctor.cs                # Doctor entity
│   ├── User.cs                  # User/auth entity
│   ├── TriageAssessment.cs      # Assessment data
│   ├── PatientAssignment.cs     # Doctor-patient link
│   ├── RiskPrediction.cs        # AI prediction result
│   ├── DepartmentPrediction.cs  # Department routing
│   └── Enums/                   # Enumerations
│       ├── TriageLevel.cs       # Emergency levels
│       ├── Department.cs        # Hospital departments
│       └── SymptomSeverity.cs   # Symptom grades
│
├── Services/                    # Business logic
│   ├── AuthService.cs           # Authentication
│   ├── PatientService.cs        # Patient operations
│   ├── DoctorService.cs         # Doctor operations
│   ├── TriageService.cs         # Triage processing
│   ├── AssignmentService.cs     # Patient-doctor matching
│   ├── ModelTrainingService.cs  # ML model training
│   ├── DepartmentAnalysisService.cs # Department routing
│   ├── PdfReportService.cs      # Report generation
│   └── BiasAnalysisResult.cs    # Fairness metrics
│
├── Platforms/                   # Platform-specific code
│   ├── Android/
│   ├── iOS/
│   ├── MacCatalyst/
│   └── Windows/
│
├── Resources/                   # App resources
│   ├── AppIcon/
│   ├── Fonts/
│   ├── Images/
│   └── Splash/
│
├── wwwroot/                     # Static web assets
│   ├── index.html
│   ├── app.css
│   └── css/
│
├── App.xaml                     # MAUI app definition
├── MainPage.xaml                # Root page
├── MauiProgram.cs               # DI configuration
└── HospitalTriageAI.csproj      # Project file
```

---

## User Roles

### 👤 Patient
- Register and login to their account
- View personal dashboard with current status
- Access triage results and risk assessment
- View assigned doctor information
- Download patient reports

### 👨‍⚕️ Doctor
- View assigned patients
- Access patient medical history
- Review triage assessments
- Update patient status

### 🔧 Admin
- Full access to all features
- Manage doctors (create, edit, delete)
- View admin dashboard with analytics
- Train and manage AI models
- Analyze model bias and fairness
- Generate reports
- Access all patient records

---

## AI/ML Features

### Triage Prediction Model

The system uses ML.NET to train a multi-class classification model that predicts patient triage levels.

#### Input Features
| Feature | Description |
|---------|-------------|
| Age | Patient age in years |
| Gender | Male/Female |
| Heart Rate | Beats per minute |
| Blood Pressure | Systolic/Diastolic mmHg |
| Temperature | Body temperature in Celsius |
| Oxygen Saturation | SpO2 percentage |
| Respiratory Rate | Breaths per minute |
| Pain Level | Scale 0-10 |
| Symptoms | Chest pain, breathing difficulty, etc. |

#### Output Classes
| Level | Description | Response Time |
|-------|-------------|---------------|
| **Emergency** | Life-threatening | Immediate |
| **Urgent** | Potentially life-threatening | Within 15 min |
| **Standard** | Serious but stable | 30-60 min |
| **Non-Urgent** | Minor conditions | 1-2 hours |

### Training Your Own Model

1. Navigate to **AI Model Management** (Admin only)
2. Prepare a CSV file with the following columns:
   ```
   Patient_ID,Age,Gender,Symptoms,Blood_Pressure,Heart_Rate,Temperature,Pre_Existing_Conditions,Risk_Level
   ```
3. Upload the CSV file
4. Click "Train Model"
5. Monitor training progress

### Bias Analysis

The platform includes comprehensive bias monitoring:

- **Gender Metrics**: Compare accuracy between male and female patients
- **Age Group Analysis**: Accuracy breakdown by age ranges (0-18, 19-40, 41-60, 61+)
- **Error Rate Analysis**: Track false positive and false negative rates
- **Fairness Score**: Overall 0-100 score indicating model fairness
  - Excellent (90-100): Minimal demographic disparities
  - Good (75-89): Acceptable fairness levels
  - Fair (60-74): Some disparities detected
  - Poor (<60): Significant bias concerns

---

## Key Components

### TriageAssessment Model
```csharp
public class TriageAssessment
{
    // Vital Signs
    public float HeartRate { get; set; }           // Normal: 60-100 bpm
    public float BloodPressureSystolic { get; set; } // Normal: 90-120 mmHg
    public float Temperature { get; set; }          // Normal: 36.1-37.2°C
    public float OxygenSaturation { get; set; }     // Normal: 95-100%
    public int PainLevel { get; set; }              // Scale: 0-10
    
    // Symptoms
    public SymptomSeverity ChestPain { get; set; }
    public SymptomSeverity ShortnessOfBreath { get; set; }
    public SymptomSeverity AlteredConsciousness { get; set; }
    
    // Results
    public TriageLevel AssignedLevel { get; set; }
    public float? AiRiskScore { get; set; }
}
```

### RiskPrediction Result
```csharp
public class RiskPrediction
{
    public TriageLevel PredictedLevel { get; set; }
    public float RiskScore { get; set; }          // 0-1 (higher = more urgent)
    public float Confidence { get; set; }         // 0-1
    public List<string> RiskFactors { get; set; }
    public string Recommendation { get; set; }
}
```

---

## Screenshots

*Coming soon - Add screenshots of key features here*

| Home Page | Patient Intake | Triage Assessment |
|-----------|----------------|-------------------|
| ![Home](screenshots/home.png) | ![Intake](screenshots/intake.png) | ![Triage](screenshots/triage.png) |

| Admin Dashboard | Doctor Management | AI Model Training |
|-----------------|-------------------|-------------------|
| ![Dashboard](screenshots/dashboard.png) | ![Doctors](screenshots/doctors.png) | ![AI](screenshots/ai.png) |

---

## Contributing

We welcome contributions! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Development Guidelines

- Follow C# coding conventions
- Write unit tests for new features
- Update documentation as needed
- Ensure cross-platform compatibility

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## Acknowledgments

- [.NET MAUI](https://dotnet.microsoft.com/apps/maui) - Cross-platform framework
- [ML.NET](https://dotnet.microsoft.com/apps/machinelearning-ai/ml-dotnet) - Machine learning framework
- [Entity Framework Core](https://docs.microsoft.com/ef/core/) - ORM
- [SQLite](https://www.sqlite.org/) - Embedded database

---

<div align="center">

**Built with ❤️ for better healthcare**

[Report Bug](https://github.com/yourusername/CareSync/issues) · [Request Feature](https://github.com/yourusername/CareSync/issues)

</div>
