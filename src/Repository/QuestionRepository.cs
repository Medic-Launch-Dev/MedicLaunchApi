using MedicLaunchApi.Data;
using MedicLaunchApi.Exceptions;
using MedicLaunchApi.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
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
                QuestionState = model.IsSubmitted ? Data.QuestionState.Submitted : Data.QuestionState.Draft
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

        public async Task<List<QuestionViewModel>> FilterQuestionsAsync(MedicLaunchApi.Models.ViewModels.QuestionsFilterRequest filterRequest, string userId)
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

            var query = from q in result
                        join n in notesQuery on q.Id equals n.QuestionId into gj
                        from note in gj.DefaultIfEmpty()
                        select CreateQuestionViewModel(q, note);

            // Apply question ordering. Ordering can be Random or by Speciality
            var questions = await query.ToListAsync();
            var order = Enum.Parse<SelectionOrder>(filterRequest.SelectionOrder);
            switch (order)
            {
                case SelectionOrder.Randomized:
                    questions = questions.OrderBy(m => Guid.NewGuid()).ToList();
                    break;
                case SelectionOrder.OrderBySpeciality:
                    questions = questions.OrderBy(m => m.SpecialityName).ToList();
                    break;
                default:
                    break;
            }

            return questions;
        }

        private static QuestionViewModel CreateQuestionViewModel(Question question, Note? note = null)
        {
            if(question.Speciality == null)
            {
                throw new InvalidOperationException("Include speciality information in question retrieval");
            }

            if(question.Options == null || question.Options.Count == 0)
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
                IsSubmitted = question.QuestionState == QuestionState.Submitted
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

        public async Task<IEnumerable<QuestionViewModel>> GetMockExamQuestionsAsync(string mockExamType)
        {
            var questionType = Enum.Parse<QuestionType>(mockExamType);
            return await dbContext.Questions.Where(q => q.QuestionType == questionType && q.QuestionState == QuestionState.Submitted)
                .Include(m => m.Speciality)
                .Include(m => m.Options)
                .Select(q => CreateQuestionViewModel(q, null)).ToListAsync();
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

        #endregion
    }
}
