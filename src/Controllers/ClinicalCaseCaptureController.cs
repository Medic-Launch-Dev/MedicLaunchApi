using MedicLaunchApi.Authorization;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MedicLaunchApi.Models;
using System.Security.Claims;

namespace MedicLaunchApi.Controllers
{
	[Route("api/clinicalCaseCapture")]
	[ApiController]
	[Authorize(Policy = AuthPolicies.RequireSubscriptionOrTrial)]
	public class ClinicalCaseCaptureController : ControllerBase
	{
		private readonly ClinicalCaseCaptureService clinicalCaseCaptureService;
		private readonly UserManager<MedicLaunchUser> userManager;

		private string CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

		public ClinicalCaseCaptureController(
			ClinicalCaseCaptureService clinicalCaseCaptureService,
			UserManager<MedicLaunchUser> userManager)
		{
			this.clinicalCaseCaptureService = clinicalCaseCaptureService;
			this.userManager = userManager;
		}

		[HttpPost("generate")]
		public async Task<IActionResult> GenerateClinicalCase([FromBody] ClinicalCaseDetails caseDetails)
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

			if (user.IsOnFreeTrial)
			{
				user.TrialClinicalCasesGeneratedCount += 1;
				await userManager.UpdateAsync(user);
			}

			return Ok(result);
		}
	}
}