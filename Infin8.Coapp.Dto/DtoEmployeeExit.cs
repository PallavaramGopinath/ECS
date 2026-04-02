using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class DtoEmployeeExit
    {
        public decimal EmployeeId { get; set; }
        public string? EmployeeNo { get; set; }
        public string? EmployeeName { get; set; }
        public string? Designation { get; set; }
        public DateTime? DateOfJoin { get; set; }
        public DateTime? DateOfExit { get; set; }
        public string? ReasonForExit { get; set; }
        public bool IsActive { get; set; } /// memberdelete -> delete or not
    }
}
