using MedicLaunchApi.Controllers;
using MedicLaunchApi.Data;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "MedicLaunchApi")
                .Options;

            logger = new Mock<ILogger<QuestionController>>().Object;

            context = new ApplicationDbContext(options);
            questionRepository = new QuestionRepository(context);
            questionController = new QuestionController(logger, questionRepository);
        }

        [TestMethod]
        public void SimpleTest()
        {
            Assert.AreEqual(1, 1);
        }

        [TestMethod]
        public async Task CreateSpeciality()
        {
            var speciality = new SpecialityViewModel()
            {
                Id = "1",
                Name = "Acute Medicine"
            };

            var result = await questionController.CreateSpeciality(speciality);
            Assert.IsNotNull(result);

            var statusCode = result as ObjectResult;
            Assert.AreEqual(200, statusCode?.StatusCode);
        }

        [TestMethod]
        public async Task CreateQuestions()
        {
            await AddTestQuestions();
        }

        [TestMethod]
        public async Task AttemptQuestions()
        {
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
            await AddTestQuestions();
            var filterRequest = new QuestionsFilterRequest()
            {
                SpecialityIds = ["1"],
                QuestionType = "General",
                Familiarity = "NewQuestions",
                SelectionOrder = "Random",
            };

            var questionsResult = await questionRepository.FilterQuestionsAsync(filterRequest, "1");
            Assert.IsNotNull(questionsResult);
            Assert.AreEqual(3, questionsResult.Count());
        }

        [TestMethod]
        public async Task FilterQuestions_IncorrectQuestions()
        {
            await AddTestQuestions();
            await AddQuestionAttempts();

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
