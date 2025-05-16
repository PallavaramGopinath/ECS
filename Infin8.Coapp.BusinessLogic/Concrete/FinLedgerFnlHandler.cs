using Infin8.Coapp.Dto;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class FinLedgerFnlHandler : IFinLedgerFnlHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public FinLedgerFnlHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<DropdownItem>> GetFinLedgerFnlListAsync()
        {
            return await _unitOfWork.FinLedgerFnl.GetFinLedgerFnlListAsync();
        }
    }
}
