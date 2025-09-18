using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class DtoSecurityDepositCreate
    {
        public decimal Employee_Id { get; set; }
        public string? Employee_Name { get; set; }
        public int Age { get; set; }
        public string? Designation { get; set; }
        
        public double Rate_Of_Interest { get; set; }
        public decimal Yr_Id { get; set; }
        public string? NomineeName { get; set; }
        public int NomineeAge { get; set; }
        public string? NomineeRelationship { get; set; }
    }
}
