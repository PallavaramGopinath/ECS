using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class TermDepositSchemeRepository : Repository<TermDeposit_Schemes>, ITermDepositSchemeRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public TermDepositSchemeRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddTermDepositSchemeAsync(TermDeposit_Schemes termDepositScheme)
        {
            bool result = false;
            try
            {
                int maxId = await CSISContext.TermDeposit_Schemes.MaxAsync(x => x.TDScheme_Id);
                maxId++;
                termDepositScheme.TDScheme_Id = maxId;
                await AddAsync(termDepositScheme);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Term deposit scheme not saved");
            }
            return result;
        }

        public async Task<bool> EditTermDepositSchemeAsync(TermDeposit_Schemes termDepositScheme)
        {
            bool result = false;
            try
            {
                //termDepositFCTemplate.TDfc_Delete = true;
                await EditAsync(termDepositScheme);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Term deposit scheme not deleted");
            }
            return result;
        }

        public async Task<List<DropdownItem>> GetTermDepositSchemeTypesAsync()
        {
            List<DropdownItem> schemeList = new List<DropdownItem>();
            try
            {
                schemeList.Add(new DropdownItem { Text = "Fixed Depsoit", Value = "3" });
                schemeList.Add(new DropdownItem {Text="Recurring Deposit",Value="4"});
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! error in fetch Term deposit scheme type");
            }
            return  await Task.FromResult(schemeList);
        }

        public async Task<List<TermDeposit_Schemes>> GetTermDepositSchemeListAsync()
        {
            List<TermDeposit_Schemes> schemeList = new List<TermDeposit_Schemes>();
            try
            {
                schemeList = await CSISContext.TermDeposit_Schemes.Where(x => x.TDScheme_Delete == false).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! error in fetch Term deposit scheme list");
            }
            return schemeList;
        }

        public async Task<List<DropdownItem>> GetTermDepositSchemeListBySchemeTypeArrayAsync(string[] schemeTypes,string brCode)
        {
            List<DropdownItem> schemeList = new List<DropdownItem>();
            try
            {
                var tdschemeList = await  CSISContext.TermDeposit_Schemes
                .Where(s => schemeTypes.Contains(s.TDSchemeType) && s.BrCode == brCode && s.TDScheme_Delete == false )
                .Select(s => new DropdownItem
                {
                    Value = s.TDScheme_Id.ToString(),
                    Text = s.TDScheme_Name
                }).ToListAsync();

                if (tdschemeList != null && tdschemeList.Count > 0) schemeList = tdschemeList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching Term deposit scheme list");
            }
            return schemeList;
        }
    }
}
