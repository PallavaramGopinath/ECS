namespace Infin8.Coapp.UI.Client.Providers
{
    using Infin8.Coapp.Dto;
    using Infin8.Coapp.Models;
    using Microsoft.AspNetCore.Components.Authorization;
    using System.Data;
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
                var response = await _http.GetAsync("api/Auth/me");

                if (!response.IsSuccessStatusCode)
                {
                    return Anonymous();
                }

                var userInfo = await response.Content.ReadFromJsonAsync<UserInfoDto>();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userInfo!.Username),
                    new Claim(ClaimTypes.NameIdentifier, userInfo.UserId.ToString()),
                    new Claim("BrCode", userInfo.BrCode! ), // Example of a custom claim)
                    new Claim("YrId", userInfo.YrId!.ToString()),
                    new Claim("YrBeginningDate", userInfo.YrBeginningDate!.ToString()),
                    new Claim("YrEndDate", userInfo.YrEndDate!.ToString ()),
                    new Claim("CurrentDate", userInfo.CurrentDate!.ToString ()),
                    new Claim("IsAuthenticated", userInfo.IsAuthenticated.ToString())
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

    //public class UserInfoDto
    //{
    //    public decimal UserId { get; set; }
    //    public string Username { get; set; } = string.Empty;
    //    public List<string> Roles { get; set; } = new List<string>();
    //    public string BrCode { get; set; }
    //    public string YrId { get; set; }
    //    public string YrBeginningDate { get; set; }
    //    public string YrEndDate { get; set; }
    //    public string CurrentDate { get; set; }
    //}
}
