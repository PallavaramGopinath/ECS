using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class RecurringDepositCreateDto
    {
        public TDCommonDto Common { get; set; } = new TDCommonDto();
        public List<TDMembersDto> MembersDtos { get; set; } = new List<TDMembersDto>();
        public int Period_In_Months { get; set; }
        public bool IsQuarterlyCompound { get; set; }
        public double Rate_Of_Interest { get; set; }
        public double Penal_Rate { get; set; }
        public double Matured_Amount { get; set; }
        public DateTime Matured_Date { get; set; }

    }
}
