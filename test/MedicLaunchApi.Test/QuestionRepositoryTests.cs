using MedicLaunchApi.Controllers;
using MedicLaunchApi.Data;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;

namespace MedicLaunchApi.Tests
{
    [TestClass]
    public class QuestionRepositoryTests
    {
        private ApplicationDbContext context;
        private QuestionRepository questionRepository;
        private QuestionController questionController;
        private ILogger<QuestionController> logger;

        [TestInitialize]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "MedicLaunchApi", new InMemoryDatabaseRoot())
                .Options;

            logger = new Mock<ILogger<QuestionController>>().Object;

            context = new ApplicationDbContext(options);
            questionRepository = new QuestionRepository(context);
            questionController = new QuestionController(logger, questionRepository);

            await AddTestQuestions();
            await AddQuestionAttempts();
        }

        private async Task AddQuestionAttempts()
        {
            var questionAttempts = new List<QuestionAttemptRequest>()
            {
                new ()
                {
                    QuestionId = "1",
                    ChosenAnswer = "A",
                    IsCorrect = true,
                    CorrectAnswer = "A",
                },
                new ()
                {
                    QuestionId = "2",
                    ChosenAnswer = "B",
                    IsCorrect = false,
                    CorrectAnswer = "C",
                },
            };

            foreach (var questionAttempt in questionAttempts)
            {
                await questionRepository.AttemptQuestionAsync(questionAttempt, "1");
            }
        }

        [TestMethod]
        public async Task FilterQuestions_NewQuestions()
        {
            //await AddTestQuestions();
            var filterRequest = new QuestionsFilterRequest()
            {
                SpecialityIds = ["1"],
                QuestionType = "General",
                Familiarity = "NewQuestions",
                SelectionOrder = "Random",
            };

            var questionsResult = await questionRepository.FilterQuestionsAsync(filterRequest, "1");
            Assert.IsNotNull(questionsResult);
            Assert.AreEqual(1, questionsResult.Count());
        }

        [TestMethod]
        public async Task FilterQuestions_IncorrectQuestions()
        {
            var filterRequest = new QuestionsFilterRequest()
            {
                SpecialityIds = ["1"],
                QuestionType = "General",
                Familiarity = "IncorrectQuestions",
                SelectionOrder = "Random",
            };

            var questionsResult = await questionRepository.FilterQuestionsAsync(filterRequest, "1");
            Assert.IsNotNull(questionsResult);
            Assert.AreEqual(1, questionsResult.Count());
        }

        [TestMethod]
        public async Task PracticeStats()
        {
            var practiceStats = await questionRepository.GetPracticeStatsAsync("1");
            Assert.IsNotNull(practiceStats);
            Assert.AreEqual(1, practiceStats.TotalIncorrect);
            Assert.AreEqual(1, practiceStats.TotalCorrect);
            Assert.AreEqual(0, practiceStats.TotalFlagged);
        }

        [TestMethod]
        public async Task AttemptQuestion_UpdateAttempt()
        {
            // Make sure an attempt is not recorded twice
            var questionAttempt = new QuestionAttemptRequest()
            {
                QuestionId = "1",
                ChosenAnswer = "A",
                IsCorrect = true,
                CorrectAnswer = "A",
            };

            await questionRepository.AttemptQuestionAsync(questionAttempt, "1");
            var practiceStats = await questionRepository.GetPracticeStatsAsync("1");
            Assert.IsNotNull(practiceStats);

            // Should still be 2 attempts, not 3
            Assert.AreEqual(2, practiceStats.TotalCorrect + practiceStats.TotalIncorrect);
        }

        #region Sample data
        private async Task AddTestQuestions()
        {
            var speciality = new Data.Speciality()
            {
                Id = "1",
                Name = "Acute Medicine"
            };

            await questionRepository.AddSpecialityAsync(speciality);

            var questions = new List<QuestionViewModel>()
            {
                new()
                {
                    Id = "1",
                    SpecialityId = "1",
                    QuestionType = "General",
                    QuestionText = "What is the capital of France?",
                    Options =
                    [
                        new() { Letter = "A", Text = "Paris" },
                        new() { Letter = "B", Text = "London" },
                        new() { Letter = "C", Text = "Berlin" }
                    ],
                    CorrectAnswerLetter = "A",
                    Explanation = "Paris is the capital of France",
                    ClinicalTips = "None",
                    LearningPoints = "None"
                },
                new()
                {
                    Id = "2",
                    SpecialityId = "1",
                    QuestionType = "General",
                    QuestionText = "What is the capital of Germany?",
                    Options =
                    [
                        new() { Letter = "A", Text = "Paris" },
                        new() { Letter = "B", Text = "London" },
                        new() { Letter = "C", Text = "Berlin" }
                    ],
                    CorrectAnswerLetter = "C",
                    Explanation = "Berlin is the capital of Germany",
                    ClinicalTips = "None",
                    LearningPoints = "None"
                },
                new()
                {
                    Id = "3",
                    SpecialityId = "1",
                    QuestionType = "General",
                    QuestionText = "What is the capital of England?",
                    Options =
                    [
                        new() { Letter = "A", Text = "Paris" },
                        new() { Letter = "B", Text = "London" },
                        new() { Letter = "C", Text = "Berlin" }
                    ],
                    CorrectAnswerLetter = "B",
                    Explanation = "London is the capital of England",
                    ClinicalTips = "None",
                    LearningPoints = "None"
                }
            };

            string userId = "1";
            foreach (var question in questions)
            {
                await questionRepository.CreateQuestionAsync(question, userId);
            }
        }
        #endregion
    }
}
