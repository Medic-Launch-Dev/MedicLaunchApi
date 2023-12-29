using MedicLaunchApi.Models;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Storage;

namespace MedicLaunchApi.Repository
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly AzureBlobClient azureBlobClient;

        public QuestionRepository(AzureBlobClient azureBlobClient)
        {
            this.azureBlobClient = azureBlobClient;
        }

        public async Task<Question> CreateQuestionAsync(Question question, CancellationToken cancellationToken)
        {
            var questionJsonPath = GetQuestionJsonPath(question.SpecialityId, question.Id);
            return await azureBlobClient.CreateItemAsync(questionJsonPath, question, cancellationToken);
        }

        public async Task<Question> UpdateQuestionAsync(Question question, CancellationToken cancellationToken)
        {
            var questionJsonPath = GetQuestionJsonPath(question.SpecialityId, question.Id);
            return await azureBlobClient.UpdateItemAsync(questionJsonPath, question, cancellationToken);
        }

        public async Task<IEnumerable<Question>> GetQuestionsAsync(string specialityId, CancellationToken cancellationToken)
        {
            var questionJsonPath = GetAllQuestionsJsonPath(specialityId);
            return await azureBlobClient.GetAllItemsAsync<Question>(questionJsonPath, cancellationToken);
        }

        public async Task DeleteQuestionAsync(string specialityId, string questionId, CancellationToken cancellationToken)
        {
            var questionJsonPath = GetQuestionJsonPath(specialityId, questionId);
            await azureBlobClient.DeleteItemAsync(questionJsonPath, cancellationToken);
        }

        public async Task<Speciality> AddSpeciality(Speciality speciality, CancellationToken cancellationToken)
        {
            string specialityPath = GetSpecialitiesJsonPath();
            var specialties = await azureBlobClient.GetItemAsync<List<Speciality>>(specialityPath, cancellationToken, true);
            if (specialties == null)
            {
                specialties = [speciality];
                await azureBlobClient.CreateItemAsync(specialityPath, specialties, cancellationToken);
                return speciality;
            }

            // update existing speciality
            specialties.Add(speciality);

            await azureBlobClient.UpdateItemAsync(specialityPath, specialties, cancellationToken);
            return speciality;
        }

        public async Task<IEnumerable<SpecialityViewModel>> GetSpecialities(CancellationToken none)
        {
            var specialityFolderPath = GetSpecialitiesJsonPath();
            var specialities = await azureBlobClient.GetItemAsync<List<Speciality>>(specialityFolderPath, none, false);
            if (specialities == null || specialities.Count() == 0)
            {
                return new List<SpecialityViewModel>();
            }

            return specialities.Select(s => new SpecialityViewModel
            {
                Id = s.Id,
                Name = s.Name
            });
        }

        public async Task AddQuestionAttempt(QuestionAttempt attempt, string userId)
        {
            string userAttemptsJsonPath = $"user/{userId}/questionattempts/{attempt.Id}.json";
            await azureBlobClient.CreateItemAsync(userAttemptsJsonPath, attempt, CancellationToken.None);
        }

        public async Task AddQuestionFlagged(FlaggedQuestion flaggedQuestion, string userId)
        {
            string userAttemptsJsonPath = $"user/{userId}/flaggedquestions/{flaggedQuestion.Id}.json";
            await azureBlobClient.CreateItemAsync(userAttemptsJsonPath, flaggedQuestion, CancellationToken.None);
        }

        public async Task<IEnumerable<FlaggedQuestion>> GetFlaggedQuestionsAsync(string userId)
        {
            var flaggedQuestionsPath = $"user/{userId}/flaggedquestions";
            return await azureBlobClient.GetAllItemsAsync<FlaggedQuestion>(flaggedQuestionsPath, CancellationToken.None);
        }

        public async Task CreateOrUpdatePracticeStats(PracticeStats practiceStats, string userId)
        {
            string userPracticeStatsJsonPath = GetPracticeStatsJsonPath(userId);
            await azureBlobClient.CreateOrUpdateItemAsync(userPracticeStatsJsonPath, practiceStats, CancellationToken.None);
        }

        public async Task<PracticeStats> GetPracticeStatsAsync(string userId)
        {
            string userPracticeStatsJsonPath = GetPracticeStatsJsonPath(userId);
            var practiceStats = await azureBlobClient.GetAllItemsAsync<PracticeStats>(userPracticeStatsJsonPath, CancellationToken.None);
            return practiceStats.FirstOrDefault();
        }

        public async Task<IEnumerable<QuestionAttempt>> GetAttemptedQuestionsAsync(string userId)
        {
            // TODO: add speciality as filter to reduce attempt files fetched
            var attemptsPath = GetUserAttemptsJsonPath(userId);
            return await azureBlobClient.GetAllItemsAsync<QuestionAttempt>(attemptsPath, CancellationToken.None);
        }

        private string GetPracticeStatsJsonPath(string userId)
        {
            return $"user/{userId}/practicestats";
        }

        private static string GetUserAttemptsJsonPath(string userId)
        {
            return $"user/{userId}/questionattempts";
        }

        private string GetQuestionJsonPath(string specialtyId, string questionId)
        {
            return $"speciality/{specialtyId}/questions/{questionId}.json";
        }

        private string GetAllQuestionsJsonPath(string specialtyId)
        {
            return $"speciality/{specialtyId}/questions";
        }

        private static string GetSpecialitiesJsonPath()
        {
            return $"speciality/specialities.json";
        }
    }
}
