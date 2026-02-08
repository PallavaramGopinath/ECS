using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class UserInfoDto
    {
        public decimal UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public string? BrCode { get; set; }
        public string? YrId { get; set; }
        public string? YrBeginningDate { get; set; }
        public string? YrEndDate { get; set; }
        public string? CurrentDate { get; set; }
    }
}
