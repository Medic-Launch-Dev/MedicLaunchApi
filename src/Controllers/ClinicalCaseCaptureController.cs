using MedicLaunchApi.Authorization;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MedicLaunchApi.Models;
using System.Security.Claims;
using MedicLaunchApi.Repository;
using System.Text.Json;

namespace MedicLaunchApi.Controllers
{
	[Route("api/clinicalCaseCapture")]
	[ApiController]
	[Authorize(Policy = AuthPolicies.RequireSubscriptionOrTrial)]
	public class ClinicalCaseCaptureController : ControllerBase
	{
		private readonly ClinicalCaseCaptureService clinicalCaseCaptureService;
		private readonly UserManager<MedicLaunchUser> userManager;
		private readonly ClinicalCaseRepository clinicalCaseRepository;

		private string CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

		public ClinicalCaseCaptureController(
			ClinicalCaseCaptureService clinicalCaseCaptureService,
			UserManager<MedicLaunchUser> userManager,
			ClinicalCaseRepository clinicalCaseRepository)
		{
			this.clinicalCaseCaptureService = clinicalCaseCaptureService;
			this.userManager = userManager;
			this.clinicalCaseRepository = clinicalCaseRepository;
		}

		[HttpPost("generate")]
		public async Task<IActionResult> GenerateClinicalCase([FromBody] ClinicalCaseGenerationRequest caseDetails)
		{
			if (caseDetails == null)
			{
				return BadRequest("Case details are required.");
			}

			var user = await userManager.FindByIdAsync(CurrentUserId);
			if (user == null)
				return Unauthorized();

			int trialLimit = 5;
			if (user.IsOnFreeTrial && user.TrialClinicalCasesGeneratedCount >= trialLimit)
				return StatusCode(403, "Trial clinical case generation limit reached.");

			var result = await clinicalCaseCaptureService.GenerateClinicalCaseAsync(caseDetails);

			// Save the generated case for the user
			await clinicalCaseRepository.CreateClinicalCaseAsync(
				CurrentUserId,
				"Untitled Case", // need to fix
				JsonSerializer.Serialize(result)
			);

			if (user.IsOnFreeTrial)
			{
				user.TrialClinicalCasesGeneratedCount += 1;
				await userManager.UpdateAsync(user);
			}

			return Ok(result);
		}

		[HttpGet("list")]
		public async Task<IActionResult> GetMyClinicalCases()
		{
			var cases = await clinicalCaseRepository.GetUserClinicalCasesAsync(CurrentUserId);
			return Ok(cases);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetMyClinicalCaseById(string id)
		{
			var clinicalCase = await clinicalCaseRepository.GetClinicalCaseByIdAsync(id, CurrentUserId);
			if (clinicalCase == null)
				return NotFound();
			return Ok(clinicalCase);
		}
	}
}