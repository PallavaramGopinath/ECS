namespace Infin8.Coapp.UI.Client.Providers
{
    using Microsoft.AspNetCore.Components.Authorization;
    using System.Net.Http.Json;
    using System.Security.Claims;

    public class CookieAuthStateProvider : AuthenticationStateProvider
    {

        private readonly HttpClient _http;

        public CookieAuthStateProvider(HttpClient http)
        {
            _http = http;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var response = await _http.GetAsync("https://localhost:7073/api/Auth/me");

                if (!response.IsSuccessStatusCode)
                {
                    return Anonymous();
                }

                var userInfo = await response.Content.ReadFromJsonAsync<UserInfo>();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userInfo!.Username),
                    new Claim(ClaimTypes.NameIdentifier, userInfo.UserId.ToString())
                };

                foreach (var role in userInfo.Roles)
                    claims.Add(new Claim(ClaimTypes.Role, role));

                var identity = new ClaimsIdentity(claims, "CookiesJwt");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch
            {
                return Anonymous();
            }
        }

        public void NotifyUserLogout()
        {
            NotifyAuthenticationStateChanged(Task.FromResult(Anonymous()));
        }

        private AuthenticationState Anonymous()
        {
            return new AuthenticationState(
                new ClaimsPrincipal(new ClaimsIdentity()));
        }
        public async Task NotifyUserAuthenticationAsync()
        {
            var authState = await GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }
    }

    public class UserInfo
    {
        public decimal UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }
}
