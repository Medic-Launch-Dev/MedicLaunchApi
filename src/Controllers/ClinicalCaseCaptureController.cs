using MedicLaunchApi.Authorization;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicLaunchApi.Controllers
{
	[Route("api/clinicalCaseCapture")]
	[ApiController]
	[Authorize(Policy = AuthPolicies.RequireSubscriptionOrTrial)]
	public class ClinicalCaseCaptureController : ControllerBase
	{
		private readonly ClinicalCaseCaptureService clinicalCaseCaptureService;

		public ClinicalCaseCaptureController(ClinicalCaseCaptureService clinicalCaseCaptureService)
		{
			this.clinicalCaseCaptureService = clinicalCaseCaptureService;
		}

		[HttpPost("generate")]
		public async Task<IActionResult> GenerateClinicalCase([FromBody] ClinicalCaseDetails caseDetails)
		{
			if (caseDetails == null)
			{
				return BadRequest("Case details are required.");
			}

			var result = await clinicalCaseCaptureService.GenerateClinicalCaseAsync(caseDetails);
			return Ok(result);
		}
	}
}