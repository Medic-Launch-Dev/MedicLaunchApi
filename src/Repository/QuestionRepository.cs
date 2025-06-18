using MedicLaunchApi.Data;
using MedicLaunchApi.Exceptions;
using MedicLaunchApi.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using PracticeStats = MedicLaunchApi.Models.PracticeStats;
namespace MedicLaunchApi.Repository
{
    public class QuestionRepository
    {
        private readonly ApplicationDbContext dbContext;

        Expression<Func<Question, bool>> NewQuestionsPredicate(string userId) => q => !dbContext.QuestionAttempts.Any(attempt => attempt.UserId == userId && attempt.QuestionId == q.Id)
            && !dbContext.FlaggedQuestions.Any(flagged => flagged.UserId == userId && flagged.QuestionId == q.Id);

        Expression<Func<Question, bool>> IncorrectQuestionsPredicate(string userId) => q => dbContext.QuestionAttempts.Any(attempt => attempt.UserId == userId && attempt.QuestionId == q.Id && !attempt.IsCorrect);

        Expression<Func<Question, bool>> FlaggedQuestionsPredicate(string userId) => q => dbContext.FlaggedQuestions.Any(flagged => flagged.UserId == userId && flagged.QuestionId == q.Id);

        public QuestionRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task CreateQuestionAsync(QuestionViewModel model, string currentUserId)
        {
            var id = model.Id ?? Guid.NewGuid().ToString();
            var questionCode = await GenerateQuestionCodeAsync(model.SpecialityId);
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
                QuestionState = model.IsSubmitted ? Data.QuestionState.Submitted : Data.QuestionState.Draft,
                VideoUrl = model.VideoUrl
            };

            dbContext.Questions.Add(question);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateQuestionAsync(QuestionViewModel model, string questionId, string userId, bool isAdmin)
        {
            var question = this.dbContext.Questions.Where(m => m.Id == questionId).Include(m => m.Options).FirstOrDefault();
            if (question == null)
            {
                throw new InvalidOperationException("Question not found");
            }

            if (question.CreatedBy != userId && !isAdmin)
            {
                throw new AccessDeniedException("You are not allowed to update this question");
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
            question.Code = await GenerateQuestionCodeAsync(model.SpecialityId);
            question.QuestionState = model.IsSubmitted ? Data.QuestionState.Submitted : Data.QuestionState.Draft;
            question.VideoUrl = model.VideoUrl;

            this.dbContext.Questions.Update(question);
            await this.dbContext.SaveChangesAsync();
        }

        public async Task<string> GenerateQuestionCodeAsync(string specialityId)
        {
            (string name, int count) = await GetQuestionCountInSpeciality(specialityId);

            string questionCode = name.Substring(0, 2).ToUpper() + (count + 1);
            return questionCode;
        }

        public async Task<IEnumerable<Question>> GetAllQuestionsAsync()
        {
            return await dbContext.Questions.ToListAsync<Question>();
        }

        public async Task<IEnumerable<QuestionViewModel>> GetQuestionsInSpecialityAsync(string specialityId)
        {
            var questions = await dbContext.Questions.Where(q => q.SpecialityId == specialityId).ToListAsync();
            return questions.Select(q => CreateQuestionViewModel(q));
        }

        public async Task<Question> GetQuestionByIdAsync(string questionId)
        {
            var question = await dbContext.Questions.FindAsync(questionId);
            return question;
        }

        public async Task<IEnumerable<QuestionViewModel>> GetQuestionsToEdit(EditQuestionsRequest request, string userId, bool isAdmin)
        {
            QuestionType questionType = Enum.Parse<QuestionType>(request.QuestionType);
            var questions = dbContext.Questions.Where(q => q.SpecialityId == request.SpecialityId && q.QuestionType == questionType)
                .Include(m => m.Speciality)
                .Include(m => m.Options)
                .AsQueryable();


            // If user is not admin, only return questions created by the user
            if (!isAdmin)
            {
                questions = questions.Where(q => q.CreatedBy == userId);
            }

            var questionsToList = await questions.ToListAsync();
            return questionsToList.Select(q => CreateQuestionViewModel(q));
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

        public async Task<List<QuestionViewModel>> FilterQuestionsAsync(QuestionsFilterRequest filterRequest, string userId)
        {
            var familiarity = Enum.Parse<Familiarity>(filterRequest.Familiarity);
            IQueryable<Question> result = Enumerable.Empty<Question>().AsQueryable();

            switch (familiarity)
            {
                case Familiarity.NewQuestions:
                    result = GetNewQuestionsAsync(filterRequest, userId);
                    break;
                case Familiarity.IncorrectQuestions:
                    result = GetIncorrectQuestionsAsync(filterRequest, userId);
                    break;
                case Familiarity.FlaggedQuestions:
                    result = GetFlaggedQuestionsAsync(filterRequest, userId);
                    break;
                case Familiarity.AllQuestions:
                    result = GetAllQuestionsAsync(filterRequest);
                    break;
            }

            var notesQuery = from note in dbContext.Notes
                             where note.UserId == userId
                             select note;

            var flaggedQuestionsForUser = dbContext.FlaggedQuestions.Where(fq => fq.UserId == userId).Select(fq => fq.QuestionId);

            var questionsWithFlaggedProperty = from q in result
                                               join fq in flaggedQuestionsForUser on q.Id equals fq into gj
                                               from flaggedQuestion in gj.DefaultIfEmpty()
                                               select new { Question = q, IsFlagged = flaggedQuestion != null };

            var query = from q in questionsWithFlaggedProperty
                        join n in notesQuery on q.Question.Id equals n.QuestionId into gj
                        from note in gj.DefaultIfEmpty()
                        select CreateQuestionViewModel(q.Question, note, q.IsFlagged);

            var questions = await query.OrderBy(q => Guid.NewGuid()).ToListAsync();
            int maxAmount = Math.Min(100, filterRequest.Amount > 0 ? filterRequest.Amount : 100);
            questions = questions.Take(maxAmount).ToList();

            var order = Enum.Parse<SelectionOrder>(filterRequest.SelectionOrder);
            if (order == SelectionOrder.OrderBySpeciality)
            {
                questions = questions.OrderBy(q => q.SpecialityName).ToList();
            }

            return questions;
        }

        private static QuestionViewModel CreateQuestionViewModel(Question question, Note? note = null, bool isFlagged = false)
        {
            if (question.Speciality == null)
            {
                throw new InvalidOperationException("Include speciality information in question retrieval");
            }

            if (question.Options == null || question.Options.Count == 0)
            {
                throw new InvalidOperationException("Include options information in question retrieval");
            }

            return new QuestionViewModel
            {
                Id = question.Id,
                SpecialityId = question.SpecialityId,
                QuestionType = question.QuestionType.ToString(),
                QuestionText = question.QuestionText,
                Options = question.Options.Select(m => new OptionViewModel()
                {
                    Letter = m.Letter,
                    Text = m.Text
                }),
                CorrectAnswerLetter = question.CorrectAnswerLetter,
                Explanation = question.Explanation,
                ClinicalTips = question.ClinicalTips,
                LearningPoints = question.LearningPoints,
                QuestionCode = question.Code,
                SpecialityName = question.Speciality.Name,
                Note = note?.Content,
                IsSubmitted = question.QuestionState == QuestionState.Submitted,
                IsFlagged = isFlagged,
                VideoUrl = question.VideoUrl
            };
        }

        private IQueryable<Question> GetNewQuestionsAsync(MedicLaunchApi.Models.ViewModels.QuestionsFilterRequest filterRequest, string userId)
        {
            IQueryable<Question> candidateQuestions = GetCandidateQuestionsAsync(filterRequest.QuestionType, filterRequest.SpecialityIds, filterRequest.AllSpecialitiesSelected);

            return candidateQuestions
                .Where(NewQuestionsPredicate(userId));
        }

        private IQueryable<Question> GetIncorrectQuestionsAsync(MedicLaunchApi.Models.ViewModels.QuestionsFilterRequest filterRequest, string userId)
        {
            IQueryable<Question> candidateQuestions = GetCandidateQuestionsAsync(filterRequest.QuestionType, filterRequest.SpecialityIds, filterRequest.AllSpecialitiesSelected);
            return candidateQuestions.Where(IncorrectQuestionsPredicate(userId));
        }

        private IQueryable<Question> GetFlaggedQuestionsAsync(MedicLaunchApi.Models.ViewModels.QuestionsFilterRequest filterRequest, string userId)
        {
            IQueryable<Question> candidateQuestions = GetCandidateQuestionsAsync(filterRequest.QuestionType, filterRequest.SpecialityIds, filterRequest.AllSpecialitiesSelected);

            return candidateQuestions
                .Where(FlaggedQuestionsPredicate(userId));
        }

        private IQueryable<Question> GetAllQuestionsAsync(MedicLaunchApi.Models.ViewModels.QuestionsFilterRequest filterRequest)
        {
            IQueryable<Question> candidateQuestions = GetCandidateQuestionsAsync(filterRequest.QuestionType, filterRequest.SpecialityIds, filterRequest.AllSpecialitiesSelected);

            return candidateQuestions;
        }

        private IQueryable<Question> GetCandidateQuestionsAsync(string questionType, string[] specialityIds, bool allSpecialitiesSelected)
        {
            var questionTypeValue = Enum.Parse<QuestionType>(questionType);
            return allSpecialitiesSelected
                ? dbContext.Questions.Where(q => q.QuestionType == questionTypeValue && q.QuestionState == QuestionState.Submitted).Include(m => m.Speciality).Include(m => m.Options)
                : dbContext.Questions.Where(q => specialityIds.Contains(q.SpecialityId) && q.QuestionType == questionTypeValue && q.QuestionState == QuestionState.Submitted).Include(m => m.Speciality).Include(m => m.Options);
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

            // Check if the user already attempted
            var existingAttempt = dbContext.QuestionAttempts.FirstOrDefault(at => at.UserId == userId && at.QuestionId == questionAttempt.QuestionId);
            if (existingAttempt != null)
            {
                // Update existing attempt and return
                existingAttempt.ChosenAnswer = questionAttempt.ChosenAnswer;
                existingAttempt.CorrectAnswer = questionAttempt.CorrectAnswer;
                existingAttempt.IsCorrect = questionAttempt.IsCorrect;
                existingAttempt.UpdatedOn = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();
                return;
            }

            dbContext.QuestionAttempts.Add(attempt);
            await dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<QuestionViewModel>> GetMockExamQuestionsAsync(string mockExamType, string userId)
        {
            var questionType = Enum.Parse<QuestionType>(mockExamType);
            var mockQuestions = dbContext.Questions.Where(q => q.QuestionType == questionType && q.QuestionState == QuestionState.Submitted)
                .Include(m => m.Speciality)
                .Include(m => m.Options)
                .AsQueryable();

            // left join mockQuestions with flagged questions
            var flaggedQuestionsForUser = dbContext.FlaggedQuestions.Where(fq => fq.UserId == userId).Select(fq => fq.QuestionId);

            var questionsWithFlaggedProperty = from q in mockQuestions
                                               join fq in flaggedQuestionsForUser on q.Id equals fq into gj
                                               from flaggedQuestion in gj.DefaultIfEmpty()
                                               select new { Question = q, IsFlagged = flaggedQuestion != null };

            var result = from q in questionsWithFlaggedProperty
                         select CreateQuestionViewModel(q.Question, null, q.IsFlagged);
            return await result.ToListAsync();
        }

        public async Task<QuestionFamiliarityCounts> GetQuestionFamiliarityCountsAsync(string userId, FamiliarityCountsRequest model)
        {
            IQueryable<Question> candidateQuestions = GetCandidateQuestionsAsync(model.QuestionType, model.SpecialityIds, model.AllSpecialitiesSelected);

            var counts = new QuestionFamiliarityCounts()
            {
                NewQuestions = await candidateQuestions.CountAsync(NewQuestionsPredicate(userId)),
                IncorrectQuestions = await candidateQuestions.CountAsync(IncorrectQuestionsPredicate(userId)),
                FlaggedQuestions = await candidateQuestions.CountAsync(FlaggedQuestionsPredicate(userId)),
                AllQuestions = await candidateQuestions.CountAsync()
            };

            return counts;
        }

        // TODO: confirm whether this includes mock exam questions
        public int GetTotalAttemptedQuestionsForUser(string userId)
        {
            return dbContext.QuestionAttempts.Count(attempt => attempt.UserId == userId);
        }

        public async Task RemoveFlaggedQuestionAsync(string questionId, string currentUserId)
        {
            var flaggedQuestion = await dbContext.FlaggedQuestions.FirstOrDefaultAsync(fq => fq.QuestionId == questionId && fq.UserId == currentUserId);
            if (flaggedQuestion == null)
            {
                return;
            }

            dbContext.FlaggedQuestions.Remove(flaggedQuestion);
            await dbContext.SaveChangesAsync();
        }

        // Reset all questions attempted by the user as well as flagged questions
        public async Task ResetUserPracticeAsync(string userId)
        {
            var attempts = dbContext.QuestionAttempts.Where(attempt => attempt.UserId == userId);
            dbContext.QuestionAttempts.RemoveRange(attempts);

            var flaggedQuestions = dbContext.FlaggedQuestions.Where(fq => fq.UserId == userId);
            dbContext.FlaggedQuestions.RemoveRange(flaggedQuestions);

            await dbContext.SaveChangesAsync();
        }

        public async Task<List<SpecialityAnalyzerResponse>> GetSpecialityAnalytics(string userId)
        {
            var allSpecialities = await dbContext.Specialities.ToListAsync();

            var attempts = from a in dbContext.QuestionAttempts
                           join b in dbContext.Questions on a.QuestionId equals b.Id
                           where a.UserId == userId
                           select new { a, b.SpecialityId, a.IsCorrect };

            var attemptsBySpeciality = await attempts
                .GroupBy(x => x.SpecialityId)
                .Select(g => new
                {
                    SpecialityId = g.Key,
                    Correct = g.Count(x => x.IsCorrect),
                    Incorrect = g.Count(x => !x.IsCorrect),
                    QuestionsAnswered = g.Count()
                })
                .ToListAsync();

            var totalQuestionsBySpeciality = await dbContext.Questions
                .GroupBy(q => q.SpecialityId)
                .Select(g => new { SpecialityId = g.Key, TotalQuestions = g.Count() })
                .ToListAsync();

            var result = allSpecialities.Select(s =>
            {
                var attempt = attemptsBySpeciality.FirstOrDefault(a => a.SpecialityId == s.Id);
                var totalQuestions = totalQuestionsBySpeciality.FirstOrDefault(t => t.SpecialityId == s.Id)?.TotalQuestions ?? 0;

                return new SpecialityAnalyzerResponse
                {
                    SpecialityId = s.Id,
                    SpecialityName = s.Name,
                    Correct = attempt?.Correct ?? 0,
                    Incorrect = attempt?.Incorrect ?? 0,
                    QuestionsAnswered = attempt?.QuestionsAnswered ?? 0,
                    TotalQuestions = totalQuestions
                };
            }).ToList();

            return result;
        }
        #endregion

        #region Trial questions
        public async Task AddTrialQuestionAsync(QuestionViewModel model, string currentUserId)
        {
            var id = model.Id ?? Guid.NewGuid().ToString();
            var questionCode = await GenerateQuestionCodeAsync(model.SpecialityId);
            var question = new MedicLaunchApi.Data.TrialQuestion
            {
                Id = id,
                SpecialityId = model.SpecialityId,
                QuestionType = Enum.Parse<Data.QuestionType>(model.QuestionType),
                QuestionText = model.QuestionText,
                Options = model.Options.Select(m => new Data.TrialAnswerOption()
                {
                    Id = Guid.NewGuid().ToString(),
                    Letter = m.Letter,
                    Text = m.Text,
                    TrialQuestionId = id
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
                QuestionState = model.IsSubmitted ? Data.QuestionState.Submitted : Data.QuestionState.Draft,
                VideoUrl = model.VideoUrl
            };

            dbContext.TrialQuestions.Add(question);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateTrialQuestionAsync(QuestionViewModel model, string questionId, string userId, bool isAdmin)
        {
            var question = this.dbContext.TrialQuestions.Where(m => m.Id == questionId).Include(m => m.Options).FirstOrDefault();
            if (question == null)
            {
                throw new InvalidOperationException("Question not found");
            }

            if (question.CreatedBy != userId && !isAdmin)
            {
                throw new AccessDeniedException("You are not allowed to update this question");
            }

            foreach (var option in question.Options)
            {
                this.dbContext.Remove(option);
            }

            question.QuestionText = model.QuestionText;
            foreach (var option in model.Options)
            {
                question.Options.Add(new Data.TrialAnswerOption()
                {
                    Id = Guid.NewGuid().ToString(),
                    Letter = option.Letter,
                    Text = option.Text,
                    TrialQuestionId = questionId
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
            question.Code = await GenerateQuestionCodeAsync(model.SpecialityId);
            question.QuestionState = model.IsSubmitted ? Data.QuestionState.Submitted : Data.QuestionState.Draft;
            question.VideoUrl = model.VideoUrl;

            this.dbContext.TrialQuestions.Update(question);
            await this.dbContext.SaveChangesAsync();
        }

        private static QuestionViewModel CreateTrialQuestionViewModel(TrialQuestion question)
        {
            if (question.Speciality == null)
            {
                throw new InvalidOperationException("Include speciality information in question retrieval");
            }

            if (question.Options == null || question.Options.Count == 0)
            {
                throw new InvalidOperationException("Include options information in question retrieval");
            }

            return new QuestionViewModel
            {
                Id = question.Id,
                SpecialityId = question.SpecialityId,
                QuestionType = question.QuestionType.ToString(),
                QuestionText = question.QuestionText,
                Options = question.Options.Select(m => new OptionViewModel()
                {
                    Letter = m.Letter,
                    Text = m.Text
                }),
                CorrectAnswerLetter = question.CorrectAnswerLetter,
                Explanation = question.Explanation,
                ClinicalTips = question.ClinicalTips,
                LearningPoints = question.LearningPoints,
                QuestionCode = question.Code,
                SpecialityName = question.Speciality.Name,
                IsSubmitted = question.QuestionState == QuestionState.Submitted,
                VideoUrl = question.VideoUrl
            };
        }

        public async Task<IEnumerable<QuestionViewModel>> GetTrialQuestionsAsync()
        {
            var questions = await dbContext.TrialQuestions.Include(m => m.Speciality).Include(m => m.Options).ToListAsync();
            return questions.Select(q => CreateTrialQuestionViewModel(q));
        }

        public async Task DeleteTrialQuestionAsync(string questionId)
        {
            var question = await dbContext.TrialQuestions.FindAsync(questionId);
            if (question == null)
            {
                throw new Exception("Question not found");
            }

            dbContext.TrialQuestions.Remove(question);
            await dbContext.SaveChangesAsync();
        }
        #endregion
    }
}
