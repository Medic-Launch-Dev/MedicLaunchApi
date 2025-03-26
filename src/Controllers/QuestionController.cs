using MedicLaunchApi.Authorization;
using MedicLaunchApi.Exceptions;
using MedicLaunchApi.Models.QuestionDTOs;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;
using MedicLaunchApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedicLaunchApi.Controllers
{
    [Route("api/questions")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly ILogger<QuestionController> logger;
        private readonly QuestionRepository questionRepository;
        private readonly IQuestionGenerationService questionGenerationService;

        public QuestionController(
            ILogger<QuestionController> logger, 
            QuestionRepository questionRepository,
            IQuestionGenerationService questionGenerationService)
        {
            this.logger = logger;
            this.questionRepository = questionRepository;
            this.questionGenerationService = questionGenerationService;
        }

		[Authorize(Policy = RoleConstants.QuestionAuthor)]
		[HttpGet("{questionId}")]
		public async Task<IActionResult> GetQuestionById(string questionId)
		{
			var question = await this.questionRepository.GetQuestionByIdAsync(questionId);

			if (question == null)
			{
				return NotFound(new { message = "Question not found" });
			}

			return Ok(question);
		}

		[Authorize(Policy = RoleConstants.QuestionAuthor)]
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

        [Authorize(Policy = RoleConstants.QuestionAuthor)]
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

        [Authorize(Policy = RoleConstants.QuestionAuthor)]
        [HttpPost("list")]
        public async Task<IEnumerable<QuestionViewModel>> EditQuestions(EditQuestionsRequest request)
        {
            bool isAdmin = User.IsInRole(RoleConstants.Admin);
            return await this.questionRepository.GetQuestionsToEdit(request, GetCurrentUserId(), isAdmin);
        }

		[Authorize(Policy = RoleConstants.QuestionAuthor)]
        [HttpDelete("delete/{specialityId}/{questionId}")]
        public async Task<IActionResult> DeleteQuestion(string specialityId, string questionId)
        {
            await this.questionRepository.DeleteQuestionAsync(questionId);
            return Ok();
        }

        [Authorize(Policy = RoleConstants.QuestionAuthor)]
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

        [Authorize(Policy = RoleConstants.QuestionAuthor)]
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

        [Authorize(Policy = RoleConstants.QuestionAuthor)]
        [HttpPost("create-trial")]
        public async Task<IActionResult> CreateTrialQuestion([FromBody] QuestionViewModel model)
        {
            // Validate that the Options list has at least 4 options. Collect model validation errors
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

            await this.questionRepository.AddTrialQuestionAsync(model, GetCurrentUserId());
            return Ok();
        }

        [Authorize(Policy = RoleConstants.QuestionAuthor)]
        [HttpPost("update-trial/{questionId}")]
        public async Task<IActionResult> UpdateTrial([FromBody] QuestionViewModel model, string questionId)
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
                await this.questionRepository.UpdateTrialQuestionAsync(model, questionId, GetCurrentUserId(), isAdmin);
                return Ok();
            }
            catch (AccessDeniedException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpGet("trial-questions")]
        public async Task<IEnumerable<QuestionViewModel>> GetTrialQuestions()
        {
            return await this.questionRepository.GetTrialQuestionsAsync();
        }

        [Authorize(Policy = RoleConstants.QuestionAuthor)]
        [HttpDelete("delete-trial/{questionId}")]
        public async Task<IActionResult> DeleteTrialQuestion(string questionId)
        {
            await this.questionRepository.DeleteTrialQuestionAsync(questionId);
            return Ok();
        }

        [Authorize(Policy = RoleConstants.QuestionAuthor)]
        [HttpPost("generate/text-and-explanation")]
        public async Task<ActionResult<QuestionTextAndExplanation>> GenerateQuestionTextAndExplanation([FromBody] string conditions)
        {
            try 
            {
                var generatedQuestion = await questionGenerationService.GenerateQuestionTextAndExplanationAsync(conditions);
                return Ok(generatedQuestion);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error generating question");
                return StatusCode(500, "Error generating question");
            }
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
