using MedicLaunchApi.Authorization;
using MedicLaunchApi.Data;
using MedicLaunchApi.Exceptions;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedicLaunchApi.Controllers
{
	[Authorize]
	[Route("api/textbooklesson")]
	[ApiController]
	public class TextbookLessonController : ControllerBase
	{
		private readonly TextbookLessonRepository textbookLessonRepository;

		public TextbookLessonController(TextbookLessonRepository textbookLessonRepository)
		{
			this.textbookLessonRepository = textbookLessonRepository;
		}

		[Authorize(Policy = RoleConstants.QuestionAuthor)]
		[HttpPost("create")]
		public async Task<IActionResult> CreateTextbookLesson([FromBody] CreateTextbookLessonRequest request)
		{
			await textbookLessonRepository.CreateTextbookLessonAsync(request, GetCurrentUserId());

			return Ok();
		}

		[Authorize(Policy = RoleConstants.QuestionAuthor)]
		[HttpPut("update")]
		public async Task<IActionResult> UpdateTextbookLesson([FromBody] UpdateTextbookLessonRequest request)
		{
			try
			{
				bool isAdmin = User.IsInRole(RoleConstants.Admin);
				var textbookLesson = await textbookLessonRepository.UpdateTextbookLessonAsync(request, GetCurrentUserId(), isAdmin);

				if (textbookLesson == null)
				{
					return NotFound();
				}

				return Ok(textbookLesson);
			}
			catch (AccessDeniedException ex)
			{
				return Forbid(ex.Message);
			}
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetTextbookLesson(string id)
		{
			var textbookLesson = await textbookLessonRepository.GetTextbookLessonAsync(id);

			if (textbookLesson == null)
			{
				return NotFound();
			}

			return Ok(textbookLesson);
		}

		[HttpGet("list")]
		public async Task<IActionResult> GetTextbookLessons([FromQuery] string? specialityId)
		{
			IEnumerable<TextbookLessonResponse> textbookLessons;

			if (!string.IsNullOrEmpty(specialityId))
			{
				textbookLessons = await textbookLessonRepository.GetTextbookLessonsBySpecialityAsync(specialityId);
				if (!textbookLessons.Any())
				{
					return NotFound("No lessons found for the given specialty.");
				}
			}
			else
			{
				textbookLessons = await textbookLessonRepository.GetTextbookLessonsAsync();
			}

			return Ok(textbookLessons);
		}

		[Authorize(Policy = RoleConstants.QuestionAuthor)] // Adjust role if necessary
		[HttpDelete("delete/{id}")]
		public async Task<IActionResult> DeleteTextbookLesson(string id)
		{
			await textbookLessonRepository.DeleteTextbookLessonAsync(id);

			return Ok();
		}

		private string GetCurrentUserId()
		{
			return User.FindFirstValue(ClaimTypes.NameIdentifier);
		}
	}
}

