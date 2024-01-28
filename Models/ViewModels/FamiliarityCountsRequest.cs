namespace MedicLaunchApi.Models.ViewModels
{
    public class FamiliarityCountsRequest
    {
        public string[] SpecialityIds { get; set; }
        public bool AllSpecialitiesSelected { get; set; }
    }
}