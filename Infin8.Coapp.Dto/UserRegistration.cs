using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class UserRegistration
    {
        [Required(ErrorMessage = "User name is required")]
        [StringLength(50)]
        public string? username { get; set; }

        [Required(ErrorMessage = "Email id is required")]
        public string? email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? password { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50)]
        public string? first_name { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50)]
        public string? last_name { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile number must be 10 digits")]
        public string? mobile_number { get; set; }

        public string? role { get; set; }
    }
}
