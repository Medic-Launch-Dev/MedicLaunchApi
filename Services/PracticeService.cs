using MedicLaunchApi.Models;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;

namespace MedicLaunchApi.Services
{
    public class PracticeService
    {
        private readonly QuestionRepository questionRepository;
        public PracticeService(QuestionRepository questionRepository)
        {
            this.questionRepository = questionRepository;
        }

        public async Task<IEnumerable<QuestionViewModel>> GetQuestions(QuestionsFilterRequest filterRequest, string currentUserId)
        {
            if(filterRequest.AllSpecialitiesSelected)
            {
                var allSpecialities = await this.questionRepository.GetSpecialities(CancellationToken.None);
                filterRequest.SpecialityIds = allSpecialities.Select(s => s.Id!).ToArray();
            }

            var tasks = filterRequest.SpecialityIds.Select(speciality => this.questionRepository.GetQuestionsAsync(speciality, CancellationToken.None));
            var questions = await Task.WhenAll(tasks);
            var questionType = Enum.Parse<QuestionType>(filterRequest.QuestionType);

            var allQuestions = questions.SelectMany(q => q).Where(m => m.QuestionType == questionType);

            var flaggedQuestions = await this.questionRepository.GetFlaggedQuestionsAsync(currentUserId);
            var attemptedQuestions = await this.questionRepository.GetAttemptedQuestionsAsync(currentUserId);

            var familiarity = Enum.Parse<Familiarity>(filterRequest.Familiarity);
            IEnumerable<Question> selectedQuestions = new List<Question>();
            switch (familiarity)
            {
                case Familiarity.NewQuestions:
                    selectedQuestions = allQuestions.Where(q => !attemptedQuestions.Any(attempt => attempt.QuestionId == q.Id));
                    break;
                case Familiarity.IncorrectQuestions:
                    selectedQuestions = allQuestions.Where(q => attemptedQuestions.Any(attempt => attempt.QuestionId == q.Id && !attempt.IsCorrect));
                    break;
                case Familiarity.FlaggedQuestions:
                    selectedQuestions = allQuestions.Where(q => flaggedQuestions.Any(flagged => flagged.QuestionId == q.Id));
                    break;
                case Familiarity.AllQuestions:
                    selectedQuestions = allQuestions;
                    break;
                default:
                    break;
            }

            var specialities = await this.questionRepository.GetSpecialities(CancellationToken.None);
            var specialityMap = specialities.ToDictionary(s => s.Id!, s => s.Name);
            var questionsList = CreateQuestionViewModel(selectedQuestions, specialityMap);
            return questionsList;
        }

        public IEnumerable<QuestionViewModel> CreateQuestionViewModel(IEnumerable<Question> questions, Dictionary<string, string> specialityMap)
        {
            return questions.Select(q => new QuestionViewModel
            {
                Id = q.Id,
                SpecialityId = q.SpecialityId,
                QuestionType = q.QuestionType.ToString(),
                QuestionText = q.QuestionText,
                Options = q.Options,
                CorrectAnswerLetter = q.CorrectAnswerLetter,
                Explanation = q.Explanation,
                ClinicalTips = q.ClinicalTips,
                LearningPoints = q.LearningPoints,
                SpecialityName = specialityMap[q.SpecialityId]
            }).ToList();
        }
    }
}
