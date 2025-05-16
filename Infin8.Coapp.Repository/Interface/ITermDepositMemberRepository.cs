using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface ITermDepositMemberRepository
    {
        Task<bool> AddTermDepositMemberAsync(TermDeposit_Members termDepositMember);
        Task<bool> EditTermDepositMemberAsync(TermDeposit_Members termDepositMember);
    }
}
