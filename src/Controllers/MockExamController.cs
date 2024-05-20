using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedicLaunchApi.Controllers
{
    [Route("api/mockexam")]
    [ApiController]
    [Authorize]
    public class MockExamController : ControllerBase
    {
        private readonly QuestionRepository questionRepository;
        private readonly MockExamRepository mockExamRepository;
        private string CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        public MockExamController(QuestionRepository questionRepository, MockExamRepository mockExamRepository)
        {
            this.questionRepository = questionRepository;
            this.mockExamRepository = mockExamRepository;
        }

        [HttpPost("start/{mockExamType}")]
        public async Task<IEnumerable<QuestionViewModel>> StartMockExam(string mockExamType)
        {
            var questions = await questionRepository.GetMockExamQuestionsAsync(mockExamType);
            await this.mockExamRepository.StartMockExamForUser(CurrentUserId, mockExamType, questions.Count());

            return questions;
        }

        [HttpPost("end")]
        public async Task<IActionResult> EndMockExam(string mockExamId, int questionsCompleted)
        {
            await this.mockExamRepository.EndMockExamForUser(CurrentUserId, mockExamId, questionsCompleted);
            return Ok();
        }
    }
}
