using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class LienTrnRepository :Repository<Lien_Trn>, ILienTrnRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public LienTrnRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddLienTrnListRepositoryAsync(List<Lien_Trn> lientrnList)
        {
            bool result = false;
            decimal maxId = 0;
            try
            {
                foreach (var lien in lientrnList)
                {
                    maxId = await CSISContext.Lien_Trn.MaxAsync(x => x.LienTr_Id);
                    maxId++;
                    lien.LienTr_Id = maxId;
                    await AddAsync(lien);
                }
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Lien trn list not saved");
            }
            return result;
        }

        public async Task<bool> EditLienTrnListRepositoryAsync(List<Lien_Trn> lientrnList)
        {
            bool result = false;
            try
            {
                foreach (var lien in lientrnList)
                {
                    lien.LienTr_Delete = true;
                    await EditAsync(lien);
                }
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Lien trn list not deleted");
            }
            return result;
        }

        public async Task<double> GetTDAmountByLoanIdListAsync(decimal[] loanIdList)
        {
            double tdAmount = 0;
            try
            {
                var _tdos = await CSISContext.Lien_Trn
                .Where(lt => loanIdList.Contains(lt.Loan_Id) && lt.LienTr_Delete == false)
                .SumAsync(lt => (double?)lt.TDTr_Amount) ?? 0;
                if(_tdos >0 ) tdAmount = _tdos;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching Term deposit amount on lien");
            }
            return tdAmount;
        }
    }
}
