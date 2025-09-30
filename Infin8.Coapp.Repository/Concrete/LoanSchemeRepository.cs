using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace Infin8.Coapp.Repository
{
    public class LoanSchemeRepository : Repository<Loan_Schemes>, ILoanSchemeRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public LoanSchemeRepository(DbContext context) : base(context)
        {
        }

        public bool AddLoanScheme(Loan_Schemes loanScheme)
        {
            int maxId = CSISContext.Loan_Schemes.Max(x => x.Scheme_Id);
            loanScheme.Scheme_Id = maxId;
            Add(loanScheme);
            return true;
        }

        public async Task<bool> AddLoanSchemeAsync(Loan_Schemes loanScheme)
        {
            bool result = false;
            try
            {
                int maxId = await CSISContext.Loan_Schemes.MaxAsync(x => x.Scheme_Id);
                maxId++;
                loanScheme.Scheme_Id = maxId;
                await AddAsync(loanScheme);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan Scheme not saved");
            }
            return result;
        }

        public bool EditLoanScheme(Loan_Schemes loanScheme)
        {
            Edit(loanScheme);
            return true;
        }

        public async Task<bool> EditLoanSchemeAsync(Loan_Schemes loanScheme)
        {
            bool result = false;
            try
            {
                loanScheme.Scheme_Delete = true;
                await EditAsync(loanScheme);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan Scheme not modified");
            }
            return result;
        }

        public List<DropdownItem> GetLoanSchemeItems(int loanType)
        {
            List<DropdownItem> items = new List<DropdownItem>();
            try
            {
                var data = from r in CSISContext.Loan_Schemes
                           where r.Loan_Type == loanType
                           select new DropdownItem
                           {
                               Value = r.Scheme_Id.ToString(),
                               Text = r.Scheme_Name
                           };
                if (data != null)
                {
                    items = data.ToList();
                }
            }
            catch (Exception)
            {
                //errorMessage = ex.Message;
            }
            return items;
        }

        public async Task<List<DropdownItem>> GetLoanSchemeItemsAsync(int loanType,string brCode)
        {
            List<DropdownItem> items = new List<DropdownItem>();
            try
            {
                var data = await (from r in CSISContext.Loan_Schemes
                                 where r.Loan_Type == loanType
                                 && r.Scheme_Delete == false
                                 && r.BrCode == brCode 
                                 select new DropdownItem
                                 {
                                     Value = r.Scheme_Id.ToString(),
                                     Text = r.Scheme_Name
                                 }).ToListAsync();
                if (data != null)
                {
                    items =  data;
                }
            }
            catch (Exception ex)
            {
                new InvalidOperationException(ex.Message + " Something went wrong! Loan Scheme not saved");
            }
            return items;
        }

        public async Task<string> GetLoanNoStartWithAsync(int loanType)
        {
            string newLoanNo = "";
            try
            {
                var data =  await CSISContext.Loan_Schemes.Where(x => x.Loan_Type == loanType).MaxAsync(x => x.LoanNoStartWith);
                if (data != null)
                {
                    newLoanNo = (data+1).ToString();
                }
            }
            catch (Exception ex)
            {
                newLoanNo = "";
                throw new InvalidOperationException(ex.Message + " Something went wrong! fetch a New loan no start with");
            }
            return newLoanNo;
        }

        public async  Task<List<DropdownItem>> GetInterestApplicationForLoanAsync()
        {
            List<DropdownItem> list = new List<DropdownItem>();
            try
            {
                list.Add(new DropdownItem { Text = "--- Select ---", Value = "-1" });
                list.Add(new DropdownItem { Text = "No Interest", Value = "1" });
                list.Add(new DropdownItem { Text = "On Principal Outstanding", Value = "2" });
                list.Add(new DropdownItem { Text = "On Non-OD Principal", Value = "3" });
                list.Add(new DropdownItem { Text = "On Disbursement (flat rate of interest)", Value = "4" });

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! error in fetch  interest application for loan");
            }
            return await Task.FromResult(list);
        }

        public async Task<List<DropdownItem>> GetPenalInterestApplicationForLoanAsync()
        {
            List<DropdownItem> list = new List<DropdownItem>();
            try
            {
                list.Add(new DropdownItem { Text = "--- Select ---", Value = "-1" });
                list.Add(new DropdownItem { Text = "No Penal Interest", Value = "1" });
                list.Add(new DropdownItem { Text = "On Principal Overdue", Value = "2" });
                list.Add(new DropdownItem { Text = "On Principal Overdue and Interest Overdue", Value = "3" });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! error in fetch  interest application for loan");
            }
            return await Task.FromResult(list);
        }

        public async Task<List<DropdownItem>> GetEMIInterestApplicationForLoanAsync()
        {
            List<DropdownItem> list = new List<DropdownItem>();
            try
            {
                list.Add(new DropdownItem { Text = "--- Select ---", Value = "-1" });
                list.Add(new DropdownItem { Text = "No EMI Interest", Value = "1" });
                list.Add(new DropdownItem { Text = "On Principal Overdue", Value = "2" });
                list.Add(new DropdownItem { Text = "On Principal Overdue and Interest Overdue", Value = "3" });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! error in fetch  EMI interest application for loan");
            }
            return await Task.FromResult(list);
        }

        public async Task<List<DropdownItem>> GetPrincipalDemandFrequencyAsync()
        {
            List<DropdownItem> list = new List<DropdownItem>();
            try
            {
                list.Add(new DropdownItem { Text = "--- Select ---", Value = "-1" });
                list.Add(new DropdownItem { Text = "No Demand", Value = "0" });
                list.Add(new DropdownItem { Text = "Monthly Demand", Value = "1" });
                list.Add(new DropdownItem { Text = "Quarterly Demand", Value = "3" });
                list.Add(new DropdownItem { Text = "Half Yearly Demand", Value = "6" });
                list.Add(new DropdownItem { Text = "Annual Demand", Value = "12" });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! error in fetch  principal demand frequenty for loan");
            }
            return await Task.FromResult(list);
        }

        public async Task<List<DropdownItem>> GetInterestDemandFrequencyAsync()
        {
            List<DropdownItem> list = new List<DropdownItem>();
            try
            {
                list.Add(new DropdownItem { Text = "--- Select ---", Value = "-1" });
                list.Add(new DropdownItem { Text = "No Demand", Value = "0" });
                list.Add(new DropdownItem { Text = "Monthly Demand", Value = "1" });
                list.Add(new DropdownItem { Text = "Quarterly Demand", Value = "3" });
                list.Add(new DropdownItem { Text = "Half Yearly Demand", Value = "6" });
                list.Add(new DropdownItem { Text = "Annual Demand", Value = "12" });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! error in fetch  interest demand frequency for loan");
            }
            return await  Task.FromResult(list);
        }

        public async Task<List<DropdownItem>> GetInstalmentTypeAsync()
        {
            List<DropdownItem> list = new List<DropdownItem>();
            try
            {
                list.Add(new DropdownItem { Text = "--- Select ---", Value = "-1" });
                list.Add(new DropdownItem { Text = "Equal Instalment", Value = "1" });
                list.Add(new DropdownItem { Text = "Equated Instalment", Value = "2" });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! error in fetch  Instalment type for loan");
            }
            return await Task.FromResult(list);
        }

        public async  Task<List<DropdownItem>> GetDisbursementTypeAsync()
        {
            List<DropdownItem> list = new List<DropdownItem>();
            try
            {
                list.Add(new DropdownItem { Text = "--- Select ---", Value = "-1" });
                list.Add(new DropdownItem { Text = "Single Disbursement", Value = "1" });
                list.Add(new DropdownItem { Text = "Multiple Disburement", Value = "2" });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! error in fetch  disburement type for loan");
            }
            return await Task.FromResult(list);
        }

        public async Task<Loan_Schemes> GetLoanSchemesAsync(int schemeId,string brCode)
        {
            Loan_Schemes loanSchemes = new Loan_Schemes();
            try
            {
                loanSchemes = await  CSISContext.Loan_Schemes.Where(x => x.Scheme_Id == schemeId && x.BrCode == brCode && x.Scheme_Delete == false).FirstAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching loan scheme details");
            }
            return loanSchemes;
        }
        

        public async Task<List<DropdownItem>> GetLoanSchemesItemsByLoanTypeArrayAsync(int[] loanTypeList)
        {
            List<DropdownItem> list = new List<DropdownItem>();
            try
            {
                var LoanSchemeList = await CSISContext.Loan_Schemes.Where(s => loanTypeList.Contains(s.Loan_Type) && s.Scheme_Delete == false) // Filter by Loan_Type and Scheme_Delete
                .Select(s => new DropdownItem
                {
                    Value = s.Scheme_Id.ToString(), 
                    Text = s.Scheme_Name
                })
                .ToListAsync();
                if(LoanSchemeList.Count > 0) list = LoanSchemeList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching loan scheme list by loan type list");
            }
            return list;
        }

        public async Task<int> GetPeriodOfLoan(int schemeId,string brCode)
        {
            int period = 0;
            try
            {
                period = await CSISContext.Loan_Schemes.Where(x=> x.Scheme_Id == schemeId && x.BrCode == brCode).Select(x=> x.MaximumPrincipalPeriod).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching period of loan from loan scheme data");
            }
            return period;
        }

        public async Task<Loan_Schemes> GetLoanSchemeByType(int loanType, string brCode)
        {
            Loan_Schemes loanSchemes = new Loan_Schemes();
            try
            {
                loanSchemes = await CSISContext.Loan_Schemes.Where(x => x.Loan_Type == loanType && x.BrCode == brCode  && x.Scheme_Delete == false).FirstAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching loan scheme details");
            }
            return loanSchemes;
        }

        public async Task<List<Loan_Schemes>> GetLoanSchemeListByType(int loanType,string brCode)
        {
            List<Loan_Schemes> loanSchemes = new List<Loan_Schemes>();
            try
            {
                var list  = await CSISContext.Loan_Schemes.Where(x => x.Loan_Type == loanType && x.BrCode == brCode  && x.Scheme_Delete == false).ToListAsync();
                if(list != null && list.Any())
                {
                    loanSchemes = list.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString() + " Something went wrong! An error occurred while fetching loan scheme details");
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching loan scheme details");
            }
            return loanSchemes;
        }
    }
}

