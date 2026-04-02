using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class PayGenInfoRepository : Repository<Pay_Gen_Info>, IPayGenInfoRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public PayGenInfoRepository(DbContext context) : base(context)
        {
        }

        public async Task<List<Pay_Gen_Info>> AddPayGenInfoAsync(Pay_Gen_Info payGenInfo)
        {
            List<Pay_Gen_Info> genInfoList = new();
            try
            {
                decimal maxId = await CSISContext.Pay_Gen_Info.MaxAsync(x => x.Pay_Info_Id);
                maxId++;
                payGenInfo.Pay_Info_Id  = maxId;
                await AddAsync(payGenInfo);
                CSISContext.SaveChanges();
                var result = await CSISContext.Pay_Gen_Info.Where(x => x.BrCode == payGenInfo.BrCode  && x.Pay_Info_Delete == false).ToListAsync();
                if (result != null && result.Count > 0) genInfoList = result.ToList();
            }
            catch (Exception ex)
            {
                genInfoList = new();
                throw new InvalidOperationException(ex.Message + " Payroll Info data not saved");
            }
            return genInfoList;
        }

        public async Task<List<Pay_Gen_Info>> EditPayGenInfoAsync(Pay_Gen_Info payGenInfo)
        {
            List<Pay_Gen_Info> genInfoList = new();
            try
            {
                await EditAsync(payGenInfo);
                CSISContext.SaveChanges();
                var result = await CSISContext.Pay_Gen_Info.Where(x => x.BrCode == payGenInfo.BrCode && x.Pay_Info_Delete == false).ToListAsync();
                if (result != null && result.Count > 0) genInfoList = result.ToList();
            }
            catch (Exception ex)
            {
                genInfoList = new();
                throw new InvalidOperationException(ex.Message + " Payroll Info data not modified");
            }
            return genInfoList;
        }

        public async Task<Pay_Gen_Info> GetPayGenInfoAsync(decimal infoId)
        {
            Pay_Gen_Info genInfo = new Pay_Gen_Info();
            try
            {
                genInfo = await  CSISContext.Pay_Gen_Info.Where(x=> x.Pay_Info_Id == infoId && x.Pay_Info_Delete == false).FirstAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " An occurred in fetching Payroll Info data not modified");
            }
            return genInfo;
        }

        public async Task<List<Pay_Gen_Info>> GetPayGenInfoListAsync(string brCode)
        {
            List<Pay_Gen_Info> genInfoList = new();
            try
            {
                var result  = await CSISContext.Pay_Gen_Info.Where(x => x.BrCode  == brCode && x.Pay_Info_Delete == false).ToListAsync();
                if (result != null && result.Count > 0) genInfoList = result.ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " An occurred in fetching Payroll Info data not modified");
            }
            return genInfoList;
        }
    }
}
