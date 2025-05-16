using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface ITermDepositFCTemplateHandler
    {
        Task<bool> AddTermDepositFCTemplateAsync(TermDeposit_FcTemplate termDepositFCTemplate);
        Task<bool> EditTermDepositFCTemplateAsync(TermDeposit_FcTemplate termDepositFCTemplate);
        Task<List<TermDeposit_FcTemplate>> GetTermDepositFCTemplateListAsync();
        Task<double> GetTDForeClosureROIAsync(int tdSchemeType, DateTime wef);
    }
}
