using System.Security.Claims;
using FluentAssertions;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Application.Tests;

public class CurrentUserAccessorTests
{
    [Fact]
    public void ClerkId_reads_the_sub_claim_from_the_current_user()
    {
        var identity = new ClaimsIdentity(new[] { new Claim("sub", "user_2abc123") });
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        var accessor = new HttpContextAccessor { HttpContext = httpContext };

        var currentUser = new CurrentUserAccessor(accessor);

        currentUser.ClerkId.Should().Be("user_2abc123");
    }

    [Fact]
    public void ClerkId_throws_when_there_is_no_authenticated_user()
    {
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var currentUser = new CurrentUserAccessor(accessor);

        var act = () => currentUser.ClerkId;

        act.Should().Throw<InvalidOperationException>();
    }
}
