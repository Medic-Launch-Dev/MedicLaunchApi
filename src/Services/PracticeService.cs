using MedicLaunchApi.Models;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;

namespace MedicLaunchApi.Services
{
    public class PracticeService
    {
        private readonly QuestionRepositoryLegacy questionRepositoryLegacy;
        private readonly QuestionRepository questionRepository;

        public PracticeService(QuestionRepositoryLegacy questionRepositoryLegacy, QuestionRepository questionRepository)
        {
            this.questionRepositoryLegacy = questionRepositoryLegacy;
            this.questionRepository = questionRepository;
        }

        public async Task<IEnumerable<QuestionViewModel>> GetQuestionsLegacy(QuestionsFilterRequest filterRequest, string currentUserId)
        {
            if (filterRequest.AllSpecialitiesSelected)
            {
                var allSpecialities = await this.questionRepositoryLegacy.GetSpecialities(CancellationToken.None);
                filterRequest.SpecialityIds = allSpecialities.Select(s => s.Id!).ToArray();
            }

            var tasks = filterRequest.SpecialityIds.Select(speciality => this.questionRepositoryLegacy.GetQuestionsAsync(speciality, CancellationToken.None));
            var questions = await Task.WhenAll(tasks);
            var questionType = Enum.Parse<QuestionType>(filterRequest.QuestionType);

            var allQuestions = questions.SelectMany(q => q).Where(m => m.QuestionType == questionType);

            var flaggedQuestions = await this.questionRepositoryLegacy.GetFlaggedQuestionsAsync(currentUserId);
            var attemptedQuestions = await this.questionRepositoryLegacy.GetAttemptedQuestionsAsync(currentUserId);

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

            var specialities = await this.questionRepositoryLegacy.GetSpecialities(CancellationToken.None);
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

        public async Task<QuestionFamiliarityCounts> GetCategoryCounts(string currentUserId, FamiliarityCountsRequest request)
        {
            var allSpecialitiesSelected = request.AllSpecialitiesSelected;
            var specialityIds = request.SpecialityIds;
            if (allSpecialitiesSelected)
            {
                var allSpecialities = await this.questionRepositoryLegacy.GetSpecialities(CancellationToken.None);
                specialityIds = allSpecialities.Select(s => s.Id!).ToArray();
            }

            var tasks = specialityIds.Select(speciality => this.questionRepositoryLegacy.GetQuestionsAsync(speciality, CancellationToken.None));
            var questions = await Task.WhenAll(tasks);

            var flaggedQuestions = await this.questionRepositoryLegacy.GetFlaggedQuestionsAsync(currentUserId);
            var attemptedQuestions = await this.questionRepositoryLegacy.GetAttemptedQuestionsAsync(currentUserId);

            var allQuestions = questions.SelectMany(q => q);

            var newQuestions = allQuestions.Where(q => !attemptedQuestions.Any(attempt => attempt.QuestionId == q.Id));
            var incorrectQuestions = allQuestions.Where(q => attemptedQuestions.Any(attempt => attempt.QuestionId == q.Id && !attempt.IsCorrect));

            var flagged = allQuestions.Where(q => flaggedQuestions.Any(flagged => flagged.QuestionId == q.Id));

            return new QuestionFamiliarityCounts()
            {
                NewQuestions = newQuestions.Count(),
                IncorrectQuestions = incorrectQuestions.Count(),
                FlaggedQuestions = flagged.Count(),
                AllQuestions = allQuestions.Count()
            };
        }
    }
}
