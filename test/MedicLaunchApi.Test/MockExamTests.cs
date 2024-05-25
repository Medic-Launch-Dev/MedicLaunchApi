using MedicLaunchApi.Data;
using MedicLaunchApi.Repository;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;

namespace MedicLaunchApi.Tests
{
    [TestClass]
    public class MockExamTests
    {
        private ApplicationDbContext context;
        private MockExamRepository mockexamRepository;
        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "MedicLaunchApi", new InMemoryDatabaseRoot())
                .Options;

            context = new ApplicationDbContext(options);
            mockexamRepository = new MockExamRepository(context);
        }

        [TestMethod]
        public async Task StartMockExamForUser_WithValidRequest_ShouldCreateMockExam()
        {
            var userId = "1";
            var mockExamType = "PaperOneMockExam";
            var questionCount = 10;

            await mockexamRepository.StartMockExamForUser(userId, mockExamType, questionCount);

            var mockExams = await context.MockExams.ToListAsync();
            Assert.AreEqual(1, mockExams.Count);
        }

        [TestMethod]
        public async Task StartMockExamForUser_WithInvalidMockExamType_ShouldThrowException()
        {
            var userId = "1";
            var mockExamType = "InvalidMockExamType";
            var questionCount = 10;

            await Assert.ThrowsExceptionAsync<ArgumentException>(() => mockexamRepository.StartMockExamForUser(userId, mockExamType, questionCount));
        }

        [TestMethod]
        public async Task EndMockExamForUser_WithValidRequest_ShouldEndMockExam()
        {
            var userId = "1";
            var mockExamType = "PaperOneMockExam";
            var questionCount = 10;

            await mockexamRepository.StartMockExamForUser(userId, mockExamType, questionCount);

            var mockExams = await context.MockExams.ToListAsync();
            var mockExam = mockExams.First();

            await mockexamRepository.EndMockExamForUser(userId, mockExam.Id, questionCount);

            mockExams = await context.MockExams.ToListAsync();
            mockExam = mockExams.First();

            Assert.IsNotNull(mockExam.CompletedOn);
            Assert.AreEqual(questionCount, mockExam.QuestionsCompleted);
        }
    }
}
