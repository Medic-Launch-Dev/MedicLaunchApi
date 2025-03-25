using MedicLaunchApi.Models.ViewModels;

namespace MedicLaunchApi.Models.QuestionDTOs
{
  public class QuestionTextAndExplanation
  {
    public string QuestionText { get; set; }
    public IEnumerable<OptionViewModel> Options { get; set; }
    public string CorrectAnswerLetter { get; set; }
    public string Explanation { get; set; }
  }
}