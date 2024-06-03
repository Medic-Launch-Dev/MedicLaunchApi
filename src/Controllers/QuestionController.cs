using MedicLaunchApi.Authorization;
using MedicLaunchApi.Exceptions;
using MedicLaunchApi.Models;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;
using MedicLaunchApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedicLaunchApi.Controllers
{
    [Authorize(Policy = RoleConstants.QuestionAuthor)]
    [Route("api/questions")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly ILogger<QuestionController> logger;
        private readonly QuestionRepository questionRepository;

        public QuestionController(ILogger<QuestionController> logger, QuestionRepository questionRepository)
        {
            this.logger = logger;
            this.questionRepository = questionRepository;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateQuestion([FromBody] QuestionViewModel model)
        {
            // Validate that the Options list has at least 4 options. Collect model validation errors
            if (model.Options != null && model.Options.Count() < 4)
            {
                ModelState.AddModelError("Options", "Question must have at least 4 options");
            }

            if(model.Options?.Any(o => string.IsNullOrWhiteSpace(o.Text)) ?? false)
            {
                ModelState.AddModelError("Options", "Each option must have text");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string currentUserId = GetCurrentUserId();
            await this.questionRepository.CreateQuestionAsync(model, currentUserId);
            return Ok();
        }

        [HttpPost("update/{questionId}")]
        public async Task<IActionResult> Update([FromBody] QuestionViewModel model, string questionId)
        {
            if (model.Options != null && model.Options.Count() < 4)
            {
                ModelState.AddModelError("Options", "Question must have at least 4 options");
            }

            if (model.Options?.Any(o => string.IsNullOrWhiteSpace(o.Text)) ?? false)
            {
                ModelState.AddModelError("Options", "Each option must have text");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                bool isAdmin = User.IsInRole(RoleConstants.Admin);
                await this.questionRepository.UpdateQuestionAsync(model, questionId, GetCurrentUserId(), isAdmin);
                return Ok();
            }
            catch (AccessDeniedException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpPost("list")]
        public async Task<IEnumerable<QuestionViewModel>> EditQuestions(EditQuestionsRequest request)
        {
            bool isAdmin = User.IsInRole(RoleConstants.Admin);
            return await this.questionRepository.GetQuestionsToEdit(request, GetCurrentUserId(), isAdmin);
        }

        [HttpDelete("delete/{specialityId}/{questionId}")]
        public async Task<IActionResult> DeleteQuestion(string specialityId, string questionId)
        {
            await this.questionRepository.DeleteQuestionAsync(questionId);
            return Ok();
        }

        [HttpPost("speciality/create")]
        public async Task<IActionResult> CreateSpeciality([FromBody] SpecialityViewModel model)
        {
            var speciality = new Data.Speciality
            {
                Id = Guid.NewGuid().ToString(),
                Name = model.Name
            };
            await this.questionRepository.AddSpecialityAsync(speciality);
            return Ok(speciality);
        }

        [HttpPost("speciality/bulk-create")]
        public async Task<IActionResult> CreateSpecialities([FromBody] IEnumerable<SpecialityViewModel> specialities)
        {
            foreach (var speciality in specialities)
            {
                await CreateSpeciality(speciality);
            }

            return Ok();
        }

        [HttpGet("specialities")]
        public async Task<IEnumerable<SpecialityViewModel>> GetSpecialities()
        {
            var specialties = await this.questionRepository.GetSpecialitiesAsync();
            return specialties.Select(s => new SpecialityViewModel
            {
                Id = s.Id,
                Name = s.Name
            });
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
