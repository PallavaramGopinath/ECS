using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;

namespace Infin8.Coapp.Repository
{
    public class BankMasterRepository :Repository<Bank_Master>, IBankMasterRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public BankMasterRepository(CSISContext context) : base(context)
        {
        }
        public async Task< List<DropdownItem>> GetBankItems()
        {
            List<DropdownItem> items = new List<DropdownItem>();
            try
            {
                var data = await  (from r in CSISContext.Bank_Master
                           select new DropdownItem
                           {
                               Value = r.Bank_ShortName,
                               Text = r.Bank_Name
                           }).ToListAsync();
                if (data != null)
                {
                    items = data.ToList();
                }
            }
            catch (Exception)
            {

                throw;
            }
            return items;
        }

        public bool AddBankMaster(Bank_Master bankMaster)
        {
            Add(bankMaster);
            return true;
        }

        public bool EditBankMaster(Bank_Master bankMaster)
        {
            Edit(bankMaster);
            return true;
        }

        public async Task<List<DropdownItem>> GetBankItemsAsync()
        {
            List<DropdownItem > items = new List<DropdownItem>();
            try
            {
                var MemBankList = await CSISContext.Bank_Master.Where(b => b.Bank_Delete == false) // Filter where Bank_Delete is 0
                .Select(b => new DropdownItem
                {
                    Value = b.Bank_ShortName, 
                    Text = b.Bank_Name
                })
                .ToListAsync();
                if(MemBankList != null) items = MemBankList.ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occured while fetching banks list");
            }
            return items;
        }
    }
}

