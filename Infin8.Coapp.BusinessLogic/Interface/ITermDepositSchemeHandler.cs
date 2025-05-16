using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface ITermDepositSchemeHandler
    {
        Task<bool> AddTermDepositSchemeAsync(TermDeposit_Schemes termDepositScheme);
        Task<bool> EditTermDepositSchemeAsync(TermDeposit_Schemes termDepositScheme);
        Task<List<DropdownItem>> GetTermDepositSchemeTypesAsync();
        Task<List<TermDeposit_Schemes>> GetTermDepositSchemeListAsync();
        Task<List<DropdownItem>> GetTermDepositSchemeListBySchemeTypeArrayAsync(string[] schemeTypes, string brCode);
    }
}
