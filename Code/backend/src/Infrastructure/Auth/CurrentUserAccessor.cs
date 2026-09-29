using Application.Common;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Auth;

public class CurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string ClerkId =>
        _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value
        ?? throw new InvalidOperationException("No authenticated user in the current request context.");
}
