using MedicLaunchApi.Controllers;
using MedicLaunchApi.Data;
using MedicLaunchApi.Models;
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
        /*
         * Using in-memory database for EF Core, write a test which does the following:
         * - Creates a specialization called Acute Medicince
         * - Add five questions to the specialization
         * - Retrieve the questions and check that there are five questions
         */

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

            // create mock of logger
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
    }
}
