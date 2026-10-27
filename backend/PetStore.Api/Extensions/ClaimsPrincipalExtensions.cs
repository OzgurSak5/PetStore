using System.Security.Claims;
using PetStore.Domain.Enums;
using PetStore.Domain.Exceptions;

namespace PetStore.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var sub = user.FindFirst("sub")?.Value;

        if(!int.TryParse(sub, out var userId))
        {
            throw new UnauthorizedException("Invalid token.");
        }

        return userId;
    }

    public static bool IsAdmin(this ClaimsPrincipal user) => user.IsInRole(nameof(UserRole.Admin));

}