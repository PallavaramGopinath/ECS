using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface ILienTrnRepository
    {
        Task<bool> AddLienTrnListRepositoryAsync(List<Lien_Trn> lientrnList);
        Task<bool> EditLienTrnListRepositoryAsync(List<Lien_Trn> lientrnList);
        Task<double> GetTDAmountByLoanIdListAsync(decimal[] loanIdList);
    }
}
