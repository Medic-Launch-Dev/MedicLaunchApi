using Google;
using MedicLaunchApi.Data;
using MedicLaunchApi.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using PracticeStats = MedicLaunchApi.Models.PracticeStats;

namespace MedicLaunchApi.Repository
{
    public class QuestionRepository
    {
        private readonly ApplicationDbContext dbContext;

        public QuestionRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // TODO: do we want to expose the data model or use a view model?
        public async Task CreateQuestion(Question question)
        {
            dbContext.Questions.Add(question);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateQuestion(Question question)
        {
            dbContext.Questions.Update(question);
            await dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Question>> GetAllQuestions()
        {
            return await dbContext.Questions.ToListAsync<Question>();
        }

        public async Task DeleteQuestion(string questionId)
        {
            var question = await dbContext.Questions.FindAsync(questionId);
            if (question == null)
            {
                throw new GoogleApiException("Question not found");
            }

            dbContext.Questions.Remove(question);
            await dbContext.SaveChangesAsync();
        }

        public async Task AddSpeciality(Speciality speciality)
        {
            dbContext.Specialities.Add(speciality);
            await dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Speciality>> GetSpecialities()
        {
            return await dbContext.Specialities.ToListAsync<Speciality>();
        }

        public async Task<Speciality> GetSpeciality(string specialityId)
        {
            return await dbContext.Specialities.FindAsync(specialityId);
        }

        public async Task AttemptQuestion(QuestionAttempt attempt)
        {
            dbContext.QuestionAttempts.Add(attempt);
            await dbContext.SaveChangesAsync();
        }

        public async Task AddFlaggedQuestion(FlaggedQuestion flaggedQuestion)
        {
            dbContext.FlaggedQuestions.Add(flaggedQuestion);
            await dbContext.SaveChangesAsync();
        }

        public async Task<PracticeStats> GetPracticeStats(string userId)
        {
            var stats = new PracticeStats()
            {
                TotalCorrect = await dbContext.QuestionAttempts.CountAsync(qa => qa.UserId == userId && qa.IsCorrect),
                TotalIncorrect = await dbContext.QuestionAttempts.CountAsync(qa => qa.UserId == userId && !qa.IsCorrect),
                TotalFlagged = await dbContext.FlaggedQuestions.CountAsync(fq => fq.UserId == userId)
            };

            return stats;
        }

        public async Task<List<Question>> FilterQuestions(MedicLaunchApi.Models.ViewModels.QuestionsFilterRequest filterRequest, string userId)
        {
            var familiarity = Enum.Parse<Familiarity>(filterRequest.Familiarity);

            return familiarity switch
            {
                Familiarity.NewQuestions => await GetNewQuestions(filterRequest, userId),
                Familiarity.IncorrectQuestions => await GetIncorrectQuestions(filterRequest, userId),
                Familiarity.FlaggedQuestions => await GetFlaggedQuestions(filterRequest, userId),
                Familiarity.AllQuestions => await GetAllQuestions(filterRequest),
                _ => [],
            };
        }

        private async Task<List<Question>> GetNewQuestions(MedicLaunchApi.Models.ViewModels.QuestionsFilterRequest filterRequest, string userId)
        {
            IQueryable<Question> candidateQuestions = GetCandidateQuestions(filterRequest);

            return await candidateQuestions
                .Where(q => !dbContext.QuestionAttempts.Any(attempt => attempt.UserId == userId && attempt.QuestionId == q.Id)
                                   && !dbContext.FlaggedQuestions.Any(flagged => flagged.UserId == userId && flagged.QuestionId == q.Id))
                .ToListAsync();
        }

        private async Task<List<Question>> GetIncorrectQuestions(MedicLaunchApi.Models.ViewModels.QuestionsFilterRequest filterRequest, string userId)
        {
            IQueryable<Question> candidateQuestions = GetCandidateQuestions(filterRequest);

            return await candidateQuestions
                .Where(q => dbContext.QuestionAttempts.Any(attempt => attempt.UserId == userId && attempt.QuestionId == q.Id && !attempt.IsCorrect))
                .ToListAsync();
        }

        private async Task<List<Question>> GetFlaggedQuestions(MedicLaunchApi.Models.ViewModels.QuestionsFilterRequest filterRequest, string userId)
        {
            IQueryable<Question> candidateQuestions = GetCandidateQuestions(filterRequest);

            return await candidateQuestions
                .Where(q => dbContext.FlaggedQuestions.Any(flagged => flagged.UserId == userId && flagged.QuestionId == q.Id))
                .ToListAsync();
        }

        private async Task<List<Question>> GetAllQuestions(MedicLaunchApi.Models.ViewModels.QuestionsFilterRequest filterRequest)
        {
            IQueryable<Question> candidateQuestions = GetCandidateQuestions(filterRequest);

            return await candidateQuestions.ToListAsync();
        }

        private IQueryable<Question> GetCandidateQuestions(QuestionsFilterRequest filterRequest)
        {
            var questionType = Enum.Parse<QuestionType>(filterRequest.QuestionType);
            return filterRequest.AllSpecialitiesSelected
                ? dbContext.Questions.Where(q => q.QuestionType == questionType)
                : dbContext.Questions.Where(q => filterRequest.SpecialityIds.Contains(q.SpecialityId) && q.QuestionType == questionType);
        }

        //public async Task<QuestionFamiliarityCounts> GetCategoryCounts(FamiliarityCountsRequest request, string currentUserId)
        //{
        //    IQueryable<Question> candidateQuestions = request.AllSpecialitiesSelected
        //        ? dbContext.Questions
        //        : dbContext.Questions.Where(q => request.SpecialityIds.Contains(q.SpecialityId));

        //    return new QuestionFamiliarityCounts()
        //    {
        //        AllQuestions = (await GetAllQuestions()).Count(),
        //        NewQuestions = (await GetNewQuestions(request, currentUserId)).Count(),


        //    };
        //}
    }
}
