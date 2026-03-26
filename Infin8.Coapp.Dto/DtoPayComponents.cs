using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class DtoPayComponents
    {
        public List<EmployeeMasterDto>? Employees { get; set; }
        public List<Pay_Components>? AllComponents { get; set; }
        public List<Pay_Component_Assignments>?  AllEmployeesAssignments { get; set; }
    }
}
