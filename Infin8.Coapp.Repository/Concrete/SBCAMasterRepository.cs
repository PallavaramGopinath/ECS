using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class SBCAMasterRepository  : Repository<SBCA_Master>, ISBCAMasterRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public SBCAMasterRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddSBCAMasterAsync(SBCA_Master sbcaMaster)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.SBCA_Master.MaxAsync(x => x.Acc_Id);
                maxId++;
                sbcaMaster.Acc_Id = maxId;
                await AddAsync(sbcaMaster);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while saving new SB Account");
            }
            return result;
        }

        public async Task<bool> EditSBCAMasterAsync(SBCA_Master sbcaMaster)
        {
            bool result = false;
            try
            {
                //termDepositFCTemplate.TDfc_Delete = true;
                await EditAsync(sbcaMaster);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while modifying SB account");
            }
            return result;
        }

        public async Task<List<DropdownItem>> GetSBCANosByMemIdAsync(decimal memId)
        {
            List<DropdownItem> list = new List<DropdownItem>();
            try
            {
                var sbAccountList = await CSISContext.SBCA_Master
                .Where(acc => acc.Mem_Id == memId && acc.Acc_Delete == false)
                .Select(acc => new DropdownItem
                {
                    Value = acc.Acc_Id.ToString(),
                    Text = acc.Acc_No
                })
                .ToListAsync();
                if(sbAccountList != null && sbAccountList.Count >0) list = sbAccountList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching SB account No(s) by member id");
            }
            return list;
        }
    }
}
