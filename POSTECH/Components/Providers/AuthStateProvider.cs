using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace TCZPOS.Components.Providers
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var isLoggedIn = await SecureStorage.Default.GetAsync("IsLoggedIn");

                if (isLoggedIn != "true")
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var username = await SecureStorage.Default.GetAsync("LoggedUser");
                var role = await SecureStorage.Default.GetAsync("UserRole");

                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, username ?? "User"),
                    new(ClaimTypes.Role, role ?? "Staff")
                };

                var identity = new ClaimsIdentity(claims, "CustomAuth");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch (Exception)
            {
                // If SecureStorage fails (common on some Windows setups), return anonymous
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public void NotifyUserLogin(string username, string role = "Staff")
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, username),
                new(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, "CustomAuth");
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public void NotifyUserLogout()
        {
            var identity = new ClaimsIdentity(); // Empty identity = Not Authorized
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }
    }
}
