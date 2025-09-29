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
    public class PayInitRepository : Repository<Pay_Init>, IPayInitRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public PayInitRepository(DbContext context) : base(context)
        {
        }

        public async Task<Pay_Init> AddPayInitAsync(Pay_Init payInit)
        {
            //bool result = false;
            try
            {
                decimal maxId = await CSISContext.Pay_Init.MaxAsync(x => x.Pay_Id);
                maxId++;
                payInit.Pay_Id = maxId;
                await AddAsync(payInit);
                //result = true;
            }
            catch (Exception ex)
            {
                //result = false;
                throw new InvalidOperationException(ex.Message + " Initialisation of payroll not saved");
            }
            //return result;
            return payInit;
        }

        public async Task<bool> EditPayInitAsync(Pay_Init payInit)
        {
            bool result = false;
            try
            {
                payInit.Pay_Delete = true;
                await EditAsync(payInit);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Initialisation of payroll not modified");
            }
            return result;
        }

        public async Task<decimal> GetPaySlipForDAArrears(DateTime fromDate, DateTime toDate, string description, string brCode)
        {
            decimal payId = 0;
            try
            {
                payId = await (CSISContext.Pay_Init.Where(x => x.From_Date == fromDate && x.To_Date == toDate && x.Pay_Des == description && x.BrCode == brCode  && x.Pay_Delete == false)).Select(x=> x.Pay_Id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return payId;
        }

        public async Task<List<DropdownItem>> GetPayIdList(decimal yearId, string payDes, string brCode)
        {
            List<DropdownItem> payIdList = new List<DropdownItem>();
            try
            {
                var result = await CSISContext.Pay_Init.Where(x => x.Pay_Delete == false && x.Yr_Id == yearId && x.Pay_Des == payDes && x.BrCode == brCode ).ToListAsync();
                var distinctValues = result.GroupBy(c => c.Pay_Id, (key, c) => c.FirstOrDefault());
                if (distinctValues != null)
                {
                    foreach (Pay_Init single in distinctValues)
                    {
                        if (payDes == "P")
                        {
                            DateTime date = new DateTime(single!.Pay_Year, single.Pay_Month, 1);
                            DropdownItem pay1 = new DropdownItem
                            {
                                Value = single.Pay_Id.ToString(),
                                Text = date.ToString("MMMM") + " " + single.Pay_Year.ToString()
                            };
                            payIdList.Add(pay1);
                        }
                        if (payDes == "D")
                        {
                            DateTime fromDate = (DateTime)single.From_Date!;
                            DateTime toDate = (DateTime)single.To_Date!;
                            DropdownItem pay = new DropdownItem
                            {
                                Value = single.Pay_Id.ToString(),
                                Text = fromDate.ToString("dd-MM-yyyy") + " to " + toDate.ToString("dd-MM-yyyy")
                            };

                            payIdList.Add(pay);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return payIdList;
        }
    }
}
