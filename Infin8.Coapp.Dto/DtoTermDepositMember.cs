using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoTermDepositMember
    {
        [Required(ErrorMessage = "Member No is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Member No must be greater than 0")]
        public decimal Mem_Id { get; set; }

        [Required(ErrorMessage = "Member No is required")]
        [StringLength(14, ErrorMessage = "Member No cannot exceed 14 characters")]
        public string? Member_No { get; set; }

        [Required(ErrorMessage = "Member Name is required")]
        [StringLength(100, ErrorMessage = "Member Name cannot exceed 100 characters")]

        public string? Member_Name { get; set; }
        [StringLength(100, ErrorMessage = "Father Name cannot exceed 100 characters")]
        public string? Father_Name { get; set; }

        [Range(1, 120, ErrorMessage = "Age must be between 0 and 120")]
        public int Age { get; set; }
    }
}
