using MedicLaunchApi.Data;
using Microsoft.EntityFrameworkCore;

namespace MedicLaunchApi.Repository
{
    public class MockExamRepository
    {
        private readonly ApplicationDbContext context;

        public MockExamRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task StartMockExamForUser(string userId, string mockExamType, int questionCount)
        {
            if (!Enum.TryParse<MockExamType>(mockExamType, out _))
            {
                throw new ArgumentException("Invalid mock exam type");
            }

            // TODO: should allow user to take a mock exam more than once?

            var mockExamEnum = Enum.Parse<MockExamType>(mockExamType);
            var mockExam = new MockExam
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                MockExamType = mockExamEnum,
                StartedOn = DateTime.UtcNow,
                TotalQuestions = questionCount
            };

            await this.context.MockExams.AddAsync(mockExam);
            await this.context.SaveChangesAsync();
        }

        public async Task EndMockExamForUser(string userId, string mockExamId, int questionsCompleted)
        {
            var mockExam = await this.context.MockExams.FindAsync(mockExamId);

            if (mockExam == null)
            {
                throw new ArgumentException("Mock exam not found");
            }

            if (mockExam.UserId != userId)
            {
                throw new ArgumentException("User does not have permission to end this mock exam");
            }

            mockExam.CompletedOn = DateTime.UtcNow;
            mockExam.QuestionsCompleted = questionsCompleted;
            await this.context.SaveChangesAsync();
        }
    }
}
