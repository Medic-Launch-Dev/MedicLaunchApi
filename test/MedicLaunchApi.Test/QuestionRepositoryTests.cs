using MedicLaunchApi.Controllers;
using MedicLaunchApi.Data;
using MedicLaunchApi.Exceptions;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace MedicLaunchApi.Test
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
            var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "MedicLaunchApi", new InMemoryDatabaseRoot())
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            logger = new Mock<ILogger<QuestionController>>().Object;

            context = new ApplicationDbContext(options);
            questionRepository = new QuestionRepository(context);
            questionController = new QuestionController(logger, questionRepository);
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
            await AddThreeTestQuestions();
            await AddQuestionAttempts();
            var filterRequest = new QuestionsFilterRequest()
            {
                SpecialityIds = ["1"],
                QuestionType = "General",
                Familiarity = "NewQuestions",
                SelectionOrder = "Randomized",
            };

            var questionsResult = await questionRepository.FilterQuestionsAsync(filterRequest, "1");
            Assert.IsNotNull(questionsResult);
            Assert.AreEqual(1, questionsResult.Count());
        }

        [TestMethod]
        public async Task FilterQuestions_IncorrectQuestions()
        {
            await AddThreeTestQuestions();
            await AddQuestionAttempts();
            var filterRequest = new QuestionsFilterRequest()
            {
                SpecialityIds = ["1"],
                QuestionType = "General",
                Familiarity = "IncorrectQuestions",
                SelectionOrder = "Randomized",
            };

            var questionsResult = await questionRepository.FilterQuestionsAsync(filterRequest, "1");
            Assert.IsNotNull(questionsResult);
            Assert.AreEqual(1, questionsResult.Count());
        }

        [TestMethod]
        public async Task PracticeStats()
        {
            await AddThreeTestQuestions();
            await AddQuestionAttempts();
            var practiceStats = await questionRepository.GetPracticeStatsAsync("1");
            Assert.IsNotNull(practiceStats);
            Assert.AreEqual(1, practiceStats.TotalIncorrect);
            Assert.AreEqual(1, practiceStats.TotalCorrect);
            Assert.AreEqual(0, practiceStats.TotalFlagged);
        }

        [TestMethod]
        public async Task AttemptQuestion_UpdateAttempt()
        {
            await AddThreeTestQuestions();
            await AddQuestionAttempts();
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


        [TestMethod]
        public async Task CreateQuestionAsync_WithValidRequest_ShouldCreateQuestion()
        {
            var speciality = new Speciality()
            {
                Id = "1",
                Name = "Acute Medicine"
            };

            await questionRepository.AddSpecialityAsync(speciality);

            var question = new QuestionViewModel()
            {
                Id = "4",
                SpecialityId = "1",
                QuestionType = "General",
                QuestionText = "What is the capital of Spain?",
                Options =
                [
                    new() { Letter = "A", Text = "Paris" },
                    new() { Letter = "B", Text = "London" },
                    new() { Letter = "C", Text = "Madrid" }
                ],
                CorrectAnswerLetter = "C",
                Explanation = "Madrid is the capital of Spain",
                ClinicalTips = "None",
                LearningPoints = "None",
                IsSubmitted = true
            };

            string userId = "1";
            await questionRepository.CreateQuestionAsync(question, userId);

            var questions = await context.Questions.ToListAsync();
            Assert.AreEqual(1, questions.Count);
        }

        [TestMethod]
        public async Task CreateQuestionAsync_WithInvalidSpecialityId_ShouldThrowException()
        {
            var question = new QuestionViewModel()
            {
                Id = "4",
                SpecialityId = "2",
                QuestionType = "General",
                QuestionText = "What is the capital of Spain?",
                Options =
                [
                    new() { Letter = "A", Text = "Paris" },
                    new() { Letter = "B", Text = "London" },
                    new() { Letter = "C", Text = "Madrid" }
                ],
                CorrectAnswerLetter = "C",
                Explanation = "Madrid is the capital of Spain",
                ClinicalTips = "None",
                LearningPoints = "None",
                IsSubmitted = true
            };

            string userId = "1";
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => questionRepository.CreateQuestionAsync(question, userId));
        }

        [TestMethod]
        public async Task UpdateQuestionAsync_WithValidRequest_ShouldUpdateQuestion()
        {
            await AddThreeTestQuestions();
            await AddQuestionAttempts();
            var question = new QuestionViewModel()
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
                LearningPoints = "None",
                IsSubmitted = true
            };

            string userId = "1";
            await questionRepository.UpdateQuestionAsync(question, "1", userId, false);

            var updatedQuestion = await context.Questions.FindAsync("1");
            Assert.AreEqual("Paris is the capital of France", updatedQuestion.Explanation);
        }

        [TestMethod]
        public async Task UpdateQuestionAsync_WithInvalidSpecialityId_ShouldThrowException()
        {
            var question = new QuestionViewModel()
            {
                Id = "1",
                SpecialityId = "2",
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
                LearningPoints = "None",
                IsSubmitted = true
            };

            string userId = "1";
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => questionRepository.UpdateQuestionAsync(question, "1", userId, false));
        }

        [TestMethod]
        public async Task FilterQuestions_ShouldReturnAllQuestionFields()
        {
            await AddThreeTestQuestions();
            await AddQuestionAttempts();
            var filterRequest = new QuestionsFilterRequest()
            {
                SpecialityIds = ["1"],
                QuestionType = "General",
                Familiarity = "AllQuestions",
                SelectionOrder = "OrderBySpeciality",
            };

            var questionsResult = await questionRepository.FilterQuestionsAsync(filterRequest, "1");
            Assert.IsNotNull(questionsResult);

            var question = questionsResult.First(m => m.Id == "1");
            Assert.AreEqual("1", question.Id);
            Assert.AreEqual("1", question.SpecialityId);
            Assert.AreEqual("General", question.QuestionType);
            Assert.AreEqual("What is the capital of France?", question.QuestionText);
            Assert.AreEqual(3, question.Options.Count());
            Assert.AreEqual("A", question.CorrectAnswerLetter);
            Assert.AreEqual("Paris is the capital of France", question.Explanation);
            Assert.AreEqual("Sample clinical tips", question.ClinicalTips);
            Assert.AreEqual("Sample learning points", question.LearningPoints);
            Assert.IsTrue(question.IsSubmitted);

            // Check each option is returned correctly
            var optionA = question.Options.First(o => o.Letter == "A");
            Assert.AreEqual("A", optionA.Letter);
            Assert.AreEqual("Paris", optionA.Text);

            var optionB = question.Options.First(o => o.Letter == "B");
            Assert.AreEqual("B", optionB.Letter);
            Assert.AreEqual("London", optionB.Text);

            var optionC = question.Options.First(o => o.Letter == "C");
            Assert.AreEqual("C", optionC.Letter);
            Assert.AreEqual("Berlin", optionC.Text);
        }

        // Check that order by speciality works
        [TestMethod]
        public async Task FilterQuestions_OrderBySpeciality()
        {
            await AddSpeciality(new Speciality()
            {
                Id = "1",
                Name = "Surgery"
            });

            await AddSpeciality(new Speciality()
            {
                Id = "2",
                Name = "Heart Disease"
            });

            var questions = new List<QuestionViewModel>
            {
                new QuestionViewModel
                {
                    Id = "1",
                    SpecialityId = "1",
                    QuestionType = "General",
                    QuestionText = "What is the most common complication after a cholecystectomy?",
                    Options =
                    [
                        new OptionViewModel { Letter = "A", Text = "Bleeding" },
                        new OptionViewModel { Letter = "B", Text = "Infection" },
                        new OptionViewModel { Letter = "C", Text = "Bile duct injury" }
                    ],
                    CorrectAnswerLetter = "C",
                    Explanation = "Bile duct injury is common.",
                    ClinicalTips = "Identify bile ducts.",
                    LearningPoints = "Know biliary anatomy.",
                    IsSubmitted = true
                },
                new QuestionViewModel
                {
                    Id = "2",
                    SpecialityId = "2",
                    QuestionType = "General",
                    QuestionText = "Which of the following is the most common symptom of coronary artery disease?",
                    Options =
                    [
                        new OptionViewModel { Letter = "A", Text = "Shortness of breath" },
                        new OptionViewModel { Letter = "B", Text = "Chest pain" },
                        new OptionViewModel { Letter = "C", Text = "Dizziness" }
                    ],
                    CorrectAnswerLetter = "B",
                    Explanation = "Chest pain is common.",
                    ClinicalTips = "Recognize angina.",
                    LearningPoints = "Early symptom management.",
                    IsSubmitted = true
                }
            };

            foreach (var question in questions)
            {
                await questionRepository.CreateQuestionAsync(question, "1");
            }

            var filterRequest = new QuestionsFilterRequest()
            {
                SpecialityIds = ["1", "2"],
                QuestionType = "General",
                Familiarity = "AllQuestions",
                SelectionOrder = "OrderBySpeciality",
            };

            var questionsResult = await questionRepository.FilterQuestionsAsync(filterRequest, "1");
            Assert.IsNotNull(questionsResult);

            // assert that the questions are ordered by speciality
            Assert.AreEqual("Heart Disease", questionsResult.ElementAt(0).SpecialityName);
            Assert.AreEqual("Surgery", questionsResult.ElementAt(1).SpecialityName);
        }

        [TestMethod]
        public async Task FilterQuestions_OnlyGeneralQuestionsAreReturned()
        {
            await AddThreeTestQuestions();
            await AddMockQuestion();

            var filterRequest = new QuestionsFilterRequest()
            {
                SpecialityIds = ["1"],
                QuestionType = "General",
                Familiarity = "AllQuestions",
                SelectionOrder = "OrderBySpeciality",
            };

            var questionsResult = await questionRepository.FilterQuestionsAsync(filterRequest, "1");
            Assert.IsNotNull(questionsResult);

            var allAreGeneralQuestions = questionsResult.All(q => q.QuestionType == "General");
            Assert.IsTrue(allAreGeneralQuestions);
        }

        [TestMethod]
        public async Task CreateQuestion_ThrowExceptionWhenQuestionTypeIsInvalid()
        {
            var question = new QuestionViewModel()
            {
                Id = "4",
                SpecialityId = "1",
                QuestionType = "Invalid",
                QuestionText = "What is the capital of Spain?",
                Options =
                [
                    new() { Letter = "A", Text = "Paris" },
                    new() { Letter = "B", Text = "London" },
                    new() { Letter = "C", Text = "Madrid" }
                ],
                CorrectAnswerLetter = "C",
                Explanation = "Madrid is the capital of Spain",
                ClinicalTips = "None",
                LearningPoints = "None",
                IsSubmitted = true
            };

            string userId = "1";
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => questionRepository.CreateQuestionAsync(question, userId));
        }

        [TestMethod]
        public async Task FilterQuestions_FlaggedQuestions()
        {
            await AddThreeTestQuestions();

            // flag third question
            await questionRepository.AddFlaggedQuestionAsync("3", "1");

            var filterRequest = new QuestionsFilterRequest()
            {
                SpecialityIds = ["1"],
                QuestionType = "General",
                Familiarity = "FlaggedQuestions",
                SelectionOrder = "Randomized",
            };

            var questionsResult = await questionRepository.FilterQuestionsAsync(filterRequest, "1");
            Assert.IsNotNull(questionsResult);
            Assert.AreEqual(1, questionsResult.Count());
        }

        // Do not flag a question twice
        [TestMethod]
        public async Task AddFlaggedQuestionAsync_ShouldNotFlagTwice()
        {
            await AddThreeTestQuestions();

            // flag third question
            await questionRepository.AddFlaggedQuestionAsync("3", "1");
            await questionRepository.AddFlaggedQuestionAsync("3", "1");

            var filterRequest = new QuestionsFilterRequest()
            {
                SpecialityIds = ["1"],
                QuestionType = "General",
                Familiarity = "FlaggedQuestions",
                SelectionOrder = "Randomized",
            };

            var questionsResult = await questionRepository.FilterQuestionsAsync(filterRequest, "1");
            Assert.IsNotNull(questionsResult);
            Assert.AreEqual(1, questionsResult.Count());
        }

        // test practice stats
        [TestMethod]
        public async Task GetPracticeStatsAsync_ShouldReturnPracticeStats()
        {
            await AddThreeTestQuestions();
            await AddQuestionAttempts();

            var practiceStats = await questionRepository.GetPracticeStatsAsync("1");
            Assert.IsNotNull(practiceStats);
            Assert.AreEqual(1, practiceStats.TotalIncorrect);
            Assert.AreEqual(1, practiceStats.TotalCorrect);
            Assert.AreEqual(0, practiceStats.TotalFlagged);
        }

        [TestMethod]
        public async Task GenerateQuestionCode_ShouldReturnQuestionCode()
        {
            await AddThreeTestQuestions();
            var questionCode = await questionRepository.GenerateQuestionCodeAsync("1");
            Assert.IsNotNull(questionCode);

            // The three test questions all belong to the Acute Medicine speciality, so the the fourth question code should be AC4
            Assert.AreEqual("AC4", questionCode);
        }

        [TestMethod]
        public async Task GetQuestionFamiliarityCountsAsync_ShouldReturnFamiliarityCounts()
        {
            await AddThreeTestQuestions();
            await AddQuestionAttempts();

            var familiarityCountRequest = new FamiliarityCountsRequest()
            {
                SpecialityIds = ["1"],
                QuestionType = "General"
            };

            var familiarityCounts = await questionRepository.GetQuestionFamiliarityCountsAsync("1", familiarityCountRequest);
            Assert.IsNotNull(familiarityCounts);
            Assert.AreEqual(1, familiarityCounts.NewQuestions);
            Assert.AreEqual(1, familiarityCounts.IncorrectQuestions);
            Assert.AreEqual(0, familiarityCounts.FlaggedQuestions);
            Assert.AreEqual(3, familiarityCounts.AllQuestions);
        }

        [TestMethod]
        [DataRow("PaperOneMockExam")]
        [DataRow("PaperTwoMockExam")]
        public async Task GetMockExamQuestionsAsync_ShouldReturnMockExamQuestions(string mockExamPaper)
        {
            await AddThreeTestQuestions();
            await AddMockQuestion(mockExamPaper);

            var questionsResult = await questionRepository.GetMockExamQuestionsAsync(mockExamPaper);
            Assert.IsNotNull(questionsResult);
            Assert.AreEqual(1, questionsResult.Count());

            var question = questionsResult.First();
            Assert.AreEqual("4", question.Id);
            Assert.AreEqual("1", question.SpecialityId);
            Assert.AreEqual(mockExamPaper, question.QuestionType);
            Assert.AreEqual("What is the capital of France?", question.QuestionText);
            Assert.AreEqual(3, question.Options.Count());
            Assert.AreEqual("A", question.CorrectAnswerLetter);
            Assert.AreEqual("Paris is the capital of France", question.Explanation);
            Assert.AreEqual("None", question.ClinicalTips);
            Assert.AreEqual("None", question.LearningPoints);
            Assert.IsTrue(question.IsSubmitted);
        }

        [TestMethod]
        public async Task UpdateQuestionAsync_WithInvalidUser_ShouldThrowException()
        {
            await AddThreeTestQuestions();
            var updateQuestionRequest = new QuestionViewModel()
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
                LearningPoints = "None",
                IsSubmitted = true
            };

            string userId = "2";
            await Assert.ThrowsExceptionAsync<AccessDeniedException>(() => questionRepository.UpdateQuestionAsync(updateQuestionRequest, "1", userId, false));
        }

        [TestMethod]
        public async Task UpdateQuestionAsync_WithAdminUser_ShouldUpdateQuestion()
        {
            await AddThreeTestQuestions();
            var updateQuestionRequest = new QuestionViewModel()
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
                LearningPoints = "None",
                IsSubmitted = true
            };

            // User id 2 is an admin and didn't create the question
            string userId = "2";
            await questionRepository.UpdateQuestionAsync(updateQuestionRequest, "2", userId, true);

            var updatedQuestion = await context.Questions.FindAsync("1");
            Assert.IsNotNull(updatedQuestion);
            Assert.AreEqual("Paris is the capital of France", updatedQuestion.Explanation);
        }

        [TestMethod]
        public async Task GetQuestionsInSpecialityAsync_ShouldReturnQuestionsInSpeciality()
        {
            var speciality = new Speciality()
            {
                Id = "1",
                Name = "Acute Medicine"
            };

            var sampleQuestionViewModel = new QuestionViewModel()
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
                ClinicalTips = "Sample clinical tips",
                LearningPoints = "Sample learning points",
                IsSubmitted = true
            };

            await questionRepository.AddSpecialityAsync(speciality);
            await questionRepository.CreateQuestionAsync(sampleQuestionViewModel, "1");


            var questions = await questionRepository.GetQuestionsInSpecialityAsync("1");
            Assert.IsNotNull(questions);
            Assert.AreEqual(1, questions.Count());

            var firstQuestion = questions.First();
            Assert.AreEqual("1", firstQuestion.Id);
            Assert.AreEqual("1", firstQuestion.SpecialityId);
            Assert.AreEqual("General", firstQuestion.QuestionType);

            // Validate question has options
            Assert.AreEqual(3, firstQuestion.Options.Count());
            Assert.AreEqual("A", firstQuestion.CorrectAnswerLetter);
            Assert.AreEqual("Paris is the capital of France", firstQuestion.Explanation);
            Assert.AreEqual("Sample clinical tips", firstQuestion.ClinicalTips);
            Assert.AreEqual("Sample learning points", firstQuestion.LearningPoints);
            Assert.IsTrue(firstQuestion.IsSubmitted);
        }

        public async Task AddSpeciality(Speciality speciality)
        {
            await questionRepository.AddSpecialityAsync(speciality);
        }

        private async Task AddMockQuestion(string mockExamPaper = "PaperOneMockExam", string id = "4")
        {
            var question = new QuestionViewModel()
            {
                Id = id,
                SpecialityId = "1",
                QuestionType = mockExamPaper,
                QuestionText = "What is the capital of France?",
                Options =
                [
                    new OptionViewModel { Letter = "A", Text = "Paris" },
                    new OptionViewModel { Letter = "B", Text = "London" },
                    new OptionViewModel { Letter = "C", Text = "Berlin" }
                ],
                CorrectAnswerLetter = "A",
                Explanation = "Paris is the capital of France",
                ClinicalTips = "None",
                LearningPoints = "None",
                IsSubmitted = true
            };

            string userId = "1";
            await questionRepository.CreateQuestionAsync(question, userId);
        }

        [TestCleanup]
        public void Cleanup()
        {
            context.Database.EnsureDeleted();
        }


        #region Sample data
        private async Task AddThreeTestQuestions()
        {
            var speciality = new Speciality()
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
                    ClinicalTips = "Sample clinical tips",
                    LearningPoints = "Sample learning points",
                    IsSubmitted = true
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
                    LearningPoints = "None",
                    IsSubmitted = true
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
                    LearningPoints = "None",
                    IsSubmitted = true
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
