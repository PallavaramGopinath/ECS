using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Models
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string[] Roles { get; set; } = Array.Empty<string>();
        public DateTime Expires { get; set; }
    }
}
