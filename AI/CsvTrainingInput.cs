using Microsoft.ML.Data;

namespace HospitalTriageAI.AI;

/// <summary>
/// Input data structure that matches the CSV file format for training
/// Columns: Patient_ID, Age, Gender, Symptoms, Blood_Pressure, Heart_Rate, Temperature, Pre_Existing_Conditions, Risk_Level
/// </summary>
public class CsvTrainingInput
{
    [LoadColumn(0)]
    public string Patient_ID { get; set; } = string.Empty;
    
    [LoadColumn(1)]
    public float Age { get; set; }
    
    [LoadColumn(2)]
    public string Gender { get; set; } = string.Empty;
    
    [LoadColumn(3)]
    public string Symptoms { get; set; } = string.Empty;
    
    [LoadColumn(4)]
    public string Blood_Pressure { get; set; } = string.Empty;
    
    [LoadColumn(5)]
    public float Heart_Rate { get; set; }
    
    [LoadColumn(6)]
    public float Temperature { get; set; }
    
    [LoadColumn(7)]
    public string Pre_Existing_Conditions { get; set; } = string.Empty;
    
    [LoadColumn(8)]
    public string Risk_Level { get; set; } = string.Empty;
}

/// <summary>
/// Transformed input for ML training with numeric features
/// </summary>
public class TransformedTrainingInput
{
    public float Age { get; set; }
    public float GenderEncoded { get; set; }
    public float SymptomCount { get; set; }
    public float SymptomSeverity { get; set; }
    public float BloodPressureSystolic { get; set; }
    public float BloodPressureDiastolic { get; set; }
    public float Heart_Rate { get; set; }
    public float Temperature { get; set; }
    public float PreExistingConditionCount { get; set; }
    public float HasDiabetes { get; set; }
    public float HasHeartDisease { get; set; }
    public float HasHypertension { get; set; }
    
    [ColumnName("Label")]
    public uint Label { get; set; }
}

/// <summary>
/// Output from the trained model prediction
/// </summary>
public class TrainingModelOutput
{
    [ColumnName("PredictedLabel")]
    public uint PredictedLabel { get; set; }
    
    [ColumnName("Score")]
    public float[] Score { get; set; } = Array.Empty<float>();
}
