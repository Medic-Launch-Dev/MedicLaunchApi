using MedicLaunchApi.Authorization;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MedicLaunchApi.Models;
using System.Security.Claims;
using MedicLaunchApi.Repository;
using Microsoft.AspNetCore.RateLimiting;
using MedicLaunchApi.Configurations;

namespace MedicLaunchApi.Controllers
{
	[Route("api/clinicalCases")]
	[ApiController]
	[Authorize(Policy = AuthPolicies.RequireSubscriptionOrTrial)]
	public class ClinicalCaseController : ControllerBase
	{
		private readonly ClinicalCaseService clinicalCaseService;
		private readonly UserManager<MedicLaunchUser> userManager;
		private readonly ClinicalCaseRepository clinicalCaseRepository;

		private string CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

		public ClinicalCaseController(
			ClinicalCaseService clinicalCaseService,
			UserManager<MedicLaunchUser> userManager,
			ClinicalCaseRepository clinicalCaseRepository)
		{
			this.clinicalCaseService = clinicalCaseService;
			this.userManager = userManager;
			this.clinicalCaseRepository = clinicalCaseRepository;
		}

		[HttpPost]
		public async Task<IActionResult> CreateClinicalCase([FromBody] ClinicalCaseDTO request)
		{
			if (request == null)
				return BadRequest("Request body is required.");

			var newCase = await clinicalCaseRepository.CreateClinicalCaseAsync(CurrentUserId, request.Title, request.CaseDetails);
			return Ok(newCase);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateClinicalCase(string id, [FromBody] ClinicalCaseDTO request)
		{
			if (request == null)
				return BadRequest("Request body is required.");

			var updatedCase = await clinicalCaseRepository.UpdateClinicalCaseAsync(id, CurrentUserId, request.Title, request.CaseDetails);
			if (updatedCase == null)
				return NotFound("Clinical case not found.");

			return Ok(updatedCase);
		}

		[HttpGet]
		public async Task<IActionResult> GetClinicalCases()
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

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteClinicalCase(string id)
		{
			var deleted = await clinicalCaseRepository.DeleteClinicalCaseAsync(id, CurrentUserId);
			if (!deleted)
				return NotFound();

			return NoContent();
		}


		[EnableRateLimiting(RateLimitingPolicies.Strict)]
		[HttpPost("generate")]
		public async Task<IActionResult> GenerateClinicalCase([FromBody] GenerateClinicalCaseDTO caseDetails)
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

			var result = await clinicalCaseService.GenerateClinicalCaseAsync(caseDetails);

			if (user.IsOnFreeTrial)
			{
				user.TrialClinicalCasesGeneratedCount += 1;
				await userManager.UpdateAsync(user);
			}

			return Ok(result);
		}
	}
}