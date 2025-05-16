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
    public class MapGeneralRepository : Repository<Map_General>, IMapGeneralRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public MapGeneralRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddMapGeneralAsync(Map_General mapGeneral)
        {
            bool result = false;
            try
            {
                await AddAsync(mapGeneral);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Mapping with business module with accounts module were not saved");
            }
            return result;
        }

        public async Task<bool> EditMapGeneralAsync(Map_General mapGeneral)
        {

            bool result = false;
            try
            {
                await EditAsync(mapGeneral);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Mapping with business module with accounts module were not modified");
            }
            return result;
        }

        public async Task<Map_General> GetMapGeneralAsync(string brCode)
        {
            Map_General mapGeneral = new Map_General();
            try
            {
                var map = await CSISContext.Map_General.Where(x => x.BrCode == brCode).FirstOrDefaultAsync();
                if(map != null) mapGeneral = map;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Somethig went wrong! An error occrrued while fetching mapped data");
            }
            return mapGeneral;
        }

        public async Task<DropdownItem> GetCashLedgerAsync(string brCode)
        {
            DropdownItem dropdownItem = new DropdownItem();
            try
            {
                var data = await (from mapGeneral in CSISContext.Map_General
                            join ledger in CSISContext.Fin_Ledger
                                on mapGeneral.ShareCapital_Led_Id equals ledger.Led_Id
                            where mapGeneral.BrCode == brCode
                            select new DropdownItem
                            {
                                Value = mapGeneral.ShareCapital_Led_Id.ToString(),
                                Text = ledger.Led_Name
                            }).FirstOrDefaultAsync();
                if (data != null) dropdownItem = data;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Somethig went wrong! An error occrrued while fetching share capital ledger id and name");
            }
            return dropdownItem;
        }

        public async Task<decimal> GetCashLedgerIdAsync(string brCode)
        {
            return await CSISContext.Map_General.Where(x=> x.BrCode == brCode).Select(x=> x.Cash_Led_Id).FirstOrDefaultAsync();
        }

        public async Task<decimal> GetShareCapitalLedIdAsync(string brCode)
        {
            return await CSISContext.Map_General.Where(x => x.BrCode == brCode).Select(x => x.ShareCapital_Led_Id).FirstOrDefaultAsync();
        }

        public async Task<decimal> GetDividendLedIdAsync(string brCode)
        {
            return await CSISContext.Map_General.Where(x => x.BrCode == brCode).Select(x => x.Dividend_Led_Id).FirstOrDefaultAsync();
        }

        public async Task<(decimal fdLedId, decimal fdIntLedId, decimal fdExcessIntPaidLedId)> GetFdLedgerIdListAsync(string brCode)
        {
            decimal fdLedId = 0;
            decimal fdIntLedId = 0;
            decimal fdExcessIntPaidLedId = 0;
            try
            {
                var map = await CSISContext.Map_General.Where(x => x.BrCode == brCode).FirstAsync();
                fdLedId = map.FD_Led_Id;
                fdIntLedId = map.FD_Int_Led_Id;
                fdExcessIntPaidLedId = map.FD_Excess_IntPaid_Id;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Somethig went wrong! An error occrrued while fetching fixed depoist ledger id items");
            }
            return (fdLedId, fdIntLedId, fdExcessIntPaidLedId);
        }

        public async Task<(decimal rdLedId, decimal rdIntLedId, decimal rdPiLedId)> GetRDLedgerIdListAsync(string brCode)
        {
            decimal rdLedId = 0;
            decimal rdIntLedId = 0;
            decimal rdPiLedId = 0;
            try
            {
                var map = await CSISContext.Map_General.Where(x => x.BrCode == brCode).FirstAsync();
                rdLedId = map.FD_Led_Id;
                rdIntLedId = map.FD_Int_Led_Id;
                rdPiLedId = map.FD_Excess_IntPaid_Id;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Somethig went wrong! An error occrrued while fetching fixed depoist ledger id items");
            }
            return (rdLedId, rdIntLedId, rdPiLedId);
        }

        public async Task<double> GetShareCapitalPercentageAsync(string brCode)
        {
            return await CSISContext.Map_General.Where(x => x.BrCode == brCode).Select(x => x.ShareCapitalPercentageOnLoanOS).FirstOrDefaultAsync();
        }
    }
}
