using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Utility
{
    public static class AuthCookieOptions
    {
        public static CookieOptions GetOptions()
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddHours(8) // or whatever
            };
        }

        public static CookieOptions GetDeleteOptions()
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddHours(-8)
            };
        }
    }
}
