using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class AuthenticatedUserDetailsDto
    {
        public decimal Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string[]? Roles { get; set; }
    }
}
