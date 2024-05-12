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

        public async Task CreateQuestionAsync(QuestionViewModel model, string currentUserId)
        {
            var id = model.Id ?? Guid.NewGuid().ToString();
            var questionCode = await GenerateQuestionCode(model.SpecialityId);
            var question = new MedicLaunchApi.Data.Question
            {
                Id = id,
                SpecialityId = model.SpecialityId,
                QuestionType = Enum.Parse<Data.QuestionType>(model.QuestionType),
                QuestionText = model.QuestionText,
                Options = model.Options.Select(m => new Data.AnswerOption()
                {
                    Id = Guid.NewGuid().ToString(),
                    Letter = m.Letter,
                    Text = m.Text,
                    QuestionId = id
                }).ToList(),
                CorrectAnswerLetter = model.CorrectAnswerLetter,
                Explanation = model.Explanation,
                ClinicalTips = model.ClinicalTips,
                LearningPoints = model.LearningPoints,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                UpdatedBy = currentUserId,
                CreatedBy = currentUserId,
                Code = questionCode,
            };

            dbContext.Questions.Add(question);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateQuestionAsync(QuestionViewModel model, string questionId, string userId)
        {
            var question = this.dbContext.Questions.Where(m => m.Id == questionId).Include(m => m.Options).FirstOrDefault();
            if (question == null)
            {
                throw new InvalidOperationException("Question not found");
            }

            foreach (var option in question.Options)
            {
                this.dbContext.Remove(option);
            }

            question.QuestionText = model.QuestionText;
            foreach (var option in model.Options)
            {
                question.Options.Add(new Data.AnswerOption()
                {
                    Id = Guid.NewGuid().ToString(),
                    Letter = option.Letter,
                    Text = option.Text,
                    QuestionId = questionId
                });
            }
            question.CorrectAnswerLetter = model.CorrectAnswerLetter;
            question.Explanation = model.Explanation;
            question.ClinicalTips = model.ClinicalTips;
            question.LearningPoints = model.LearningPoints;
            question.UpdatedOn = DateTime.UtcNow;
            question.UpdatedBy = userId;
            question.SpecialityId = model.SpecialityId;
            question.QuestionType = Enum.Parse<Data.QuestionType>(model.QuestionType);
            question.Code = await GenerateQuestionCode(model.SpecialityId);

            this.dbContext.Questions.Update(question);
            await this.dbContext.SaveChangesAsync();
        }

        public async Task<string> GenerateQuestionCode(string specialityId)
        {
            (string name, int count) = await GetQuestionCountInSpeciality(specialityId);

            string questionCode = name.Substring(0, 2).ToUpper() + (count + 1);
            return questionCode;
        }

        public async Task<IEnumerable<Question>> GetAllQuestionsAsync()
        {
            return await dbContext.Questions.ToListAsync<Question>();
        }

        public async Task<IEnumerable<Question>> GetQuestionsInSpecialityAsync(string specialityId)
        {
            return await dbContext.Questions.Where(q => q.SpecialityId == specialityId).Include(m => m.Options).Include(s => s.Speciality).ToListAsync();
        }

        public async Task<Question> GetQuestionAsync(string questionId)
        {
            return await dbContext.Questions.Where(q => q.Id == questionId).Include(m => m.Options).FirstOrDefaultAsync();
        }

        public async Task DeleteQuestionAsync(string questionId)
        {
            var question = await dbContext.Questions.FindAsync(questionId);
            if (question == null)
            {
                throw new Exception("Question not found");
            }

            dbContext.Questions.Remove(question);
            await dbContext.SaveChangesAsync();
        }

        public async Task AddSpecialityAsync(Speciality speciality)
        {
            dbContext.Specialities.Add(speciality);
            await dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Speciality>> GetSpecialitiesAsync()
        {
            return await dbContext.Specialities.ToListAsync<Speciality>();
        }

        public async Task<Speciality> GetSpecialityAsync(string specialityId)
        {
            return await dbContext.Specialities.FindAsync(specialityId);
        }

        public async Task<(string, int)> GetQuestionCountInSpeciality(string specialityId)
        {
            var result = dbContext.Specialities.Where(m => m.Id == specialityId).Select(m => new { Name = m.Name, QuestionCount = m.Questions.Count });

            var name = await result.Select(m => m.Name).FirstAsync();
            var count = await result.Select(m => m.QuestionCount).FirstAsync();
            return (name, count);
        }

        public async Task AddFlaggedQuestionAsync(string questionId, string userId)
        {
            var flaggedQuestion = new FlaggedQuestion()
            {
                Id = Guid.NewGuid().ToString(),
                QuestionId = questionId,
                UserId = userId,
                CreatedOn = DateTime.UtcNow
            };

            // Check if question is already flagged
            if (await dbContext.FlaggedQuestions.AnyAsync(fq => fq.QuestionId == questionId && fq.UserId == userId))
            {
                return;
            }

            dbContext.FlaggedQuestions.Add(flaggedQuestion);
            await dbContext.SaveChangesAsync();
        }

        public async Task<PracticeStats> GetPracticeStatsAsync(string userId)
        {
            var stats = new PracticeStats()
            {
                TotalCorrect = await dbContext.QuestionAttempts.CountAsync(qa => qa.UserId == userId && qa.IsCorrect),
                TotalIncorrect = await dbContext.QuestionAttempts.CountAsync(qa => qa.UserId == userId && !qa.IsCorrect),
                TotalFlagged = await dbContext.FlaggedQuestions.CountAsync(fq => fq.UserId == userId)
            };

            return stats;
        }

        public async Task<List<Question>> FilterQuestionsAsync(MedicLaunchApi.Models.ViewModels.QuestionsFilterRequest filterRequest, string userId)
        {
            var familiarity = Enum.Parse<Familiarity>(filterRequest.Familiarity);

            return familiarity switch
            {
                Familiarity.NewQuestions => await GetNewQuestionsAsync(filterRequest, userId),
                Familiarity.IncorrectQuestions => await GetIncorrectQuestionsAsync(filterRequest, userId),
                Familiarity.FlaggedQuestions => await GetFlaggedQuestionsAsync(filterRequest, userId),
                Familiarity.AllQuestions => await GetAllQuestionsAsync(filterRequest),
                _ => [],
            };
        }

        private async Task<List<Question>> GetNewQuestionsAsync(MedicLaunchApi.Models.ViewModels.QuestionsFilterRequest filterRequest, string userId)
        {
            IQueryable<Question> candidateQuestions = GetCandidateQuestionsAsync(filterRequest);

            return await candidateQuestions
                .Where(q => !dbContext.QuestionAttempts.Any(attempt => attempt.UserId == userId && attempt.QuestionId == q.Id)
                                   && !dbContext.FlaggedQuestions.Any(flagged => flagged.UserId == userId && flagged.QuestionId == q.Id))
                .ToListAsync();
        }

        private async Task<List<Question>> GetIncorrectQuestionsAsync(MedicLaunchApi.Models.ViewModels.QuestionsFilterRequest filterRequest, string userId)
        {
            IQueryable<Question> candidateQuestions = GetCandidateQuestionsAsync(filterRequest);

            return await candidateQuestions
                .Where(q => dbContext.QuestionAttempts.Any(attempt => attempt.UserId == userId && attempt.QuestionId == q.Id && !attempt.IsCorrect))
                .ToListAsync();
        }

        private async Task<List<Question>> GetFlaggedQuestionsAsync(MedicLaunchApi.Models.ViewModels.QuestionsFilterRequest filterRequest, string userId)
        {
            IQueryable<Question> candidateQuestions = GetCandidateQuestionsAsync(filterRequest);

            return await candidateQuestions
                .Where(q => dbContext.FlaggedQuestions.Any(flagged => flagged.UserId == userId && flagged.QuestionId == q.Id))
                .ToListAsync();
        }

        private async Task<List<Question>> GetAllQuestionsAsync(MedicLaunchApi.Models.ViewModels.QuestionsFilterRequest filterRequest)
        {
            IQueryable<Question> candidateQuestions = GetCandidateQuestionsAsync(filterRequest);

            return await candidateQuestions.ToListAsync();
        }

        private IQueryable<Question> GetCandidateQuestionsAsync(QuestionsFilterRequest filterRequest)
        {
            var questionType = Enum.Parse<QuestionType>(filterRequest.QuestionType);
            return filterRequest.AllSpecialitiesSelected
                ? dbContext.Questions.Where(q => q.QuestionType == questionType).Include(m => m.Speciality)
                : dbContext.Questions.Where(q => filterRequest.SpecialityIds.Contains(q.SpecialityId) && q.QuestionType == questionType).Include(m => m.Speciality);
        }

        #region Practice related methods

        public async Task AttemptQuestionAsync(QuestionAttemptRequest questionAttempt, string userId)
        {
            var attempt = new QuestionAttempt()
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                QuestionId = questionAttempt.QuestionId,
                ChosenAnswer = questionAttempt.ChosenAnswer,
                CorrectAnswer = questionAttempt.CorrectAnswer,
                IsCorrect = questionAttempt.IsCorrect,
                CreatedOn = DateTime.UtcNow
            };

            dbContext.QuestionAttempts.Add(attempt);
            await dbContext.SaveChangesAsync();
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

        #endregion
    }
}
