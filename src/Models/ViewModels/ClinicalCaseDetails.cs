namespace MedicLaunchApi.Models.ViewModels
{
  public class ClinicalCaseDetails
  {
    public string PatientDemographics { get; set; }
    public string ClinicalContext { get; set; } = string.Empty;
    public string PresentingComplaint { get; set; } = string.Empty;
    public string Symptoms { get; set; } = string.Empty;
    public string ComplaintHistory { get; set; } = string.Empty;
  }
}