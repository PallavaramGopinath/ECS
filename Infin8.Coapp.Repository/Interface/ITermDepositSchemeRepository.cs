using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface ITermDepositSchemeRepository
    {
        Task<List<TermDeposit_Schemes>> AddTermDepositSchemeAsync(TermDeposit_Schemes termDepositScheme);
        Task<List<TermDeposit_Schemes>> EditTermDepositSchemeAsync(TermDeposit_Schemes termDepositScheme);
        Task<List<DropdownItem>> GetTermDepositSchemeTypesAsync();
        Task<TermDeposit_Schemes> GetTermDepositSchemeByIdAsync(int schemeId);
        Task<List<TermDeposit_Schemes>> GetTermDepositSchemeListAsync(string brCode);
        Task<List<DropdownItem>> GetTermDepositSchemeListBySchemeTypeArrayAsync(string[] schemeTypes,string brCode);

    }
}
