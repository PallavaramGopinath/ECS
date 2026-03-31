using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
        public decimal YrId { get; set; }
        public DateTime YrBeginningDate { get; set; }
        public DateTime YrEndDate { get; set; }
        public DateTime  CurrentDate { get; set; }
        public bool IsAuthenticated { get; set; }
        public string? CalendarStatus { get; set; }
    }
}
