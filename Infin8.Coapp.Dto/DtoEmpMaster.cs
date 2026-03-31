using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public  class DtoEmpMaster
    {
        /// Employee Master
        public  Emp_Master? Employee{ get; set; } = new Emp_Master();

        ///  Member Master properties
        public MemberRegistration? Member { get; set; }= new MemberRegistration();
        public DateTime doj { get; set; }
        public mem_master? MemberMaster { get; set; } = new mem_master();
        public List<Emp_Qualification>? Qualifications { get; set; } = new List<Emp_Qualification>();
    }
}
