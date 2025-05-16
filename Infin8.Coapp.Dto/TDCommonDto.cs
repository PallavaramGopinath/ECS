using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Dto
{
    public class TDCommonDto
    {
        [Required(ErrorMessage = "Scheme Name is required.")]
        public int Tdscheme_Id { get; set; }
        [Required(ErrorMessage = "Mode of operation is required.")]

        public int Mode_Of_Operation { get; set; }
        [Required(ErrorMessage = "Account open date is required.")]
        public DateTime Account_Opendate { get; set; }
        [Required(ErrorMessage = "Value date is required.")]
        public DateTime Value_Date { get; set; }
        public decimal Mem_Id_ToAdd { get; set; }
        public string? Member_No_ToAdd { get; set; } 
        public string? Member_Name_ToAdd { get; set; } 
        public string? Father_Name_ToAdd { get; set; } 
        public int Age_ToAdd { get; set; }
        [Required(ErrorMessage = "Deposit Number is required.")]
        public string? Td_No { get; set; }
        [StringLength(125, ErrorMessage = "Depositer(s) Name cannot exceed 125 characters.")]
        public string? Tdh_Name { get; set; } 
        [StringLength(125, ErrorMessage = "First Nominee name cannot exceed 125 characters.")]
        public string? Nominee1Name { get; set; } 
        [Range(0, 150, ErrorMessage = "First Nominee age must be between 0 and 150.")]
        public int Nominee1Age { get; set; }
        [StringLength(50, ErrorMessage = "First Nominee relationship cannot exceed 50 characters.")]
        public string? Nominee1Relationship { get; set; } 
        [StringLength(125, ErrorMessage = "Second Nominee name cannot exceed 125 characters.")]
        public string? Nominee2Name { get; set; } 
        [Range(0, 150, ErrorMessage = "Second Nominee age must be between 0 and 150.")]
        public int Nominee2Age { get; set; }
        [StringLength(50, ErrorMessage = "Second Nominee relationship cannot exceed 50 characters.")]
        public string? Nominee2Relationship { get; set; } 
    }
}
