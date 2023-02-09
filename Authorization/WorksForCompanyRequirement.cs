using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Tweetbook.Authorization;
public class WorksForCompanyRequirement : IAuthorizationRequirement
{
    public string DomainName { get; }

    public WorksForCompanyRequirement(string domainName)
    {
        DomainName = domainName;
    }
}

public class WorksForCompanyHandler : AuthorizationHandler<WorksForCompanyRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        WorksForCompanyRequirement requirement)
    {
        var userEmailAddress = context.User?.FindFirstValue(ClaimTypes.Email) ??
            string.Empty;

        if (userEmailAddress.EndsWith(requirement.DomainName))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        context.Fail();
        return Task.CompletedTask;
    }

}
