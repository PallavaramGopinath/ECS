using Infin8.Coapp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IFinLedgerFnlHandler
    {
        Task<List<DropdownItem>> GetFinLedgerFnlListAsync();
    }
}
