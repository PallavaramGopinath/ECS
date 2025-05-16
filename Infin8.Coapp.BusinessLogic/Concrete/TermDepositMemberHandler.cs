using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class TermDepositMemberHandler : ITermDepositMemberHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public TermDepositMemberHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddTermDepositMemberAsync(TermDeposit_Members termDepositMember)
        {
            return await _unitOfWork.TermDepositMember.AddTermDepositMemberAsync(termDepositMember);
        }

        public async Task<bool> EditTermDepositMemberAsync(TermDeposit_Members termDepositMember)
        {
            return await _unitOfWork.TermDepositMember.EditTermDepositMemberAsync(termDepositMember);
        }
    }
}
