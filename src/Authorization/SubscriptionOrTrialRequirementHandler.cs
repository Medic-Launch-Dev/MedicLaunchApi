using MedicLaunchApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace MedicLaunchApi.Authorization
{
	public class SubscriptionOrTrialRequirementHandler : AuthorizationHandler<SubscriptionOrTrialRequirement>, IAuthorizationHandler
	{
		private readonly UserManager<MedicLaunchUser> _userManager;

		public SubscriptionOrTrialRequirementHandler(UserManager<MedicLaunchUser> userManager)
		{
			_userManager = userManager;
		}

		protected override async Task HandleRequirementAsync(
			AuthorizationHandlerContext context,
			SubscriptionOrTrialRequirement requirement)
		{
			var user = await _userManager.GetUserAsync(context.User);
			if (user == null) return;

			if (context.User.IsInRole(RoleConstants.Admin) ||
				context.User.IsInRole(RoleConstants.QuestionAuthor)
				|| context.User.IsInRole(RoleConstants.FlashcardAuthor))
			{
				context.Succeed(requirement);
				return;
			}

			if (user.HasActiveSubscription || user.IsOnFreeTrial)
			{
				context.Succeed(requirement);
				return;
			}
		}
	}
}