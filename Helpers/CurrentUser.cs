using System.Security.Claims;

namespace SmartTaskEngine.Helpers
{
    public static class CurrentUser
    {
        public static int Id(ClaimsPrincipal user) =>
            int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub")
                ?? throw new InvalidOperationException("User id claim missing from token."));
    }
}
