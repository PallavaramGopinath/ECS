using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class TermDepositTrnRepository : Repository<TermDeposit_Trn>, ITermDepositTrnRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public TermDepositTrnRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddTermDepositTrnAsync(TermDeposit_Trn termDepositTrn)
        {
            bool result = false;
            int maxSlNo = 0;
            try
            {
                decimal maxId = await CSISContext.TermDeposit_Trn.MaxAsync(x => x.TDTrn_Id);
                maxSlNo = Get_MaxTDTrnSlNo(termDepositTrn.TD_Id);
                maxId++;
                termDepositTrn.TDTrn_Id = maxId;
                termDepositTrn.Trn_SlNo = maxSlNo;
                await AddAsync(termDepositTrn);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Term deposit trn not saved");
            }
            return result;
        }

        public async Task<bool> AddTermDepositTrnListAsync(List<TermDeposit_Trn> termDepositTrnList)
        {
            bool result = false;
            int maxSlNo = 0;
            try
            {
                decimal maxId = CSISContext.TermDeposit_Trn.Max(x => x.TDTrn_Id);
                foreach (var td in termDepositTrnList)
                {
                    maxId++;
                    maxSlNo = Get_MaxTDTrnSlNo(td.TD_Id);
                    td.TDTrn_Id = maxId;
                    td.Trn_SlNo = maxSlNo;
                    await AddAsync(td);
                }
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Term Deposit Payment not saved");
            }
            return result;
        }

        public async Task<bool> EditTermDepositTrnAsync(TermDeposit_Trn termDepositTrn)
        {
            bool result = false;
            try
            {
                //termDepositFCTemplate.TDfc_Delete = true;
                await EditAsync(termDepositTrn);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Term deposit trn not deleted");
            }
            return result;
        }

        public async Task<List<DropdownItem>> GetTDNosByMemIdAsync(decimal memId, string tdSchemeType, string brCode)
        {
            List<DropdownItem> tdNos = new List<DropdownItem>();
            try
            {
                var tdNoList = await (from td in CSISContext.TermDeposit_Master
                                      join scheme in CSISContext.TermDeposit_Schemes
                                          on td.TDScheme_Id equals scheme.TDScheme_Id
                                      join member in CSISContext.TermDeposit_Members
                                          on td.TD_Id equals member.TD_Id
                                      where td.TD_Delete == false
                                          && td.AccountClosed == false
                                          && member.TDMem_Delete == false
                                          && member.Mem_Id == memId
                                          && scheme.TDSchemeType == tdSchemeType
                                          && td.BrCode == brCode
                                      orderby td.TD_No
                                      select new DropdownItem
                                      {
                                          Value = td.TD_Id.ToString(),
                                          Text = td.TD_No
                                      }).ToListAsync();
                if (tdNoList != null && tdNoList.Count > 0) tdNos = tdNoList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching Term deposit nos by member id");
            }
            return tdNos;
        }

        public async Task<List<FDDetailsVM>> GetFDPayableByTDIdsAsync(decimal[] fdNos)
        {
            List<FDDetailsVM> fdDetails = new List<FDDetailsVM>();
            try
            {
                var fdList = await (from master in CSISContext.TermDeposit_Master
                                    join trn in CSISContext.TermDeposit_Trn
                                        on master.TD_Id equals trn.TD_Id
                                    join scheme in CSISContext.TermDeposit_Schemes
                                        on master.TDScheme_Id equals scheme.TDScheme_Id
                                    where trn.TD_Delete == false
                          && master.AccountClosed == false
                                        && fdNos.Contains(master.TD_Id)
                                    group new
                                    {
                                        master,
                                        scheme,
                                        trn
                                    } by new
                                    {
                                        master.TD_Id,
                                        master.TD_No,
                                        scheme.TDScheme_Name,
                                        master.TDScheme_Id,
                                        master.ValueDate,
                                        master.DepositAmount,
                                        master.PeriodInMonths,
                                        master.PeriodInDays,
                                        master.InterestPayableFrequency,
                                        master.CompoundFrequency,
                                        master.RateOfInterest,
                                        master.IsDiscountRate,
                                        master.MaturityAmount,
                                        master.MaturityDate
                                    } into grouped
                                    select new FDDetailsVM
                                    {
                                        FDId = grouped.Key.TD_Id,
                                        TDScheme_Name = grouped.Key.TDScheme_Name,
                                        FDNo = grouped.Key.TD_No,
                                        FDSchemeId = grouped.Key.TDScheme_Id,
                                        FDValueDate = grouped.Key.ValueDate,
                                        FDAmount = grouped.Key.DepositAmount,
                                        FDPrdInMonths = grouped.Key.PeriodInMonths,
                                        FDPrdInDays = grouped.Key.PeriodInDays,
                                        FDIntPayableFrequency = grouped.Key.InterestPayableFrequency,
                                        FDCompoundFrequency = grouped.Key.CompoundFrequency,
                                        FDROI = grouped.Key.RateOfInterest,
                                        FDIsDiscountRate = grouped.Key.IsDiscountRate,
                                        FDMaturityAmount = grouped.Key.MaturityAmount,
                                        FDMaturityDate = grouped.Key.MaturityDate,
                                        FDIntAlreadyCalculated = grouped.Sum(g => g.trn.InterestCalculatedAmount),
                                        FDIntAlreadyCalculatedDate = grouped.Max(g => g.trn.InterestAppliedDate),
                                        FDIntAlreadyPaid = grouped.Sum(g => g.trn.InterestPaidAmount)
                                    }).ToListAsync();
                if (fdList!= null && fdList.Any()) fdDetails = fdList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching Term deposit payable data");
            }
            return fdDetails;
        }

        public async Task<FDDetailsVM> GetFDDataByTDId(decimal tdId,string brCode)
        {
            FDDetailsVM fdData = new FDDetailsVM();
            try
            {
                var query = await (from master in CSISContext.TermDeposit_Master
                                    join trn in CSISContext.TermDeposit_Trn
                                        on master.TD_Id equals trn.TD_Id
                                    join scheme in CSISContext.TermDeposit_Schemes
                                        on master.TDScheme_Id equals scheme.TDScheme_Id
                                    where trn.TD_Delete == false
                          && master.AccountClosed == false
                                        && master.TD_Id == tdId
                                        && master.BrCode == brCode 
                                        && scheme.BrCode == brCode
                                    group new
                                    {
                                        master,
                                        scheme,
                                        trn
                                    } by new
                                    {
                                       master.TD_Id,
                                       master.TD_No,
                                       master.TDH_Name,
                                       master.TDH_Age ,
                                       scheme.TDScheme_Name,
                                       master.TDScheme_Id,
                                       master.ValueDate,
                                       master.DepositAmount,
                                       master.PeriodInMonths,
                                       master.PeriodInDays,
                                       master.InterestPayableFrequency,
                                       master.CompoundFrequency,
                                       master.RateOfInterest,
                                       master.IsDiscountRate,
                                       master.MaturityAmount,
                                       master.MaturityDate
                                    } into grouped
                                    select new FDDetailsVM
                                    {
                                        FDId = grouped.Key.TD_Id,
                                        TDScheme_Name = grouped.Key.TDScheme_Name,
                                        TDH_Name = grouped.Key.TDH_Name,
                                        TDH_Age = grouped.Key.TDH_Age ,
                                        FDNo = grouped.Key.TD_No,
                                        FDSchemeId = grouped.Key.TDScheme_Id,
                                        FDValueDate = grouped.Key.ValueDate,
                                        FDAmount = grouped.Key.DepositAmount,
                                        FDPrdInMonths = grouped.Key.PeriodInMonths,
                                        FDPrdInDays = grouped.Key.PeriodInDays,
                                        FDIntPayableFrequency = grouped.Key.InterestPayableFrequency,
                                        FDCompoundFrequency = grouped.Key.CompoundFrequency,
                                        FDROI = grouped.Key.RateOfInterest,
                                        FDIsDiscountRate = grouped.Key.IsDiscountRate,
                                        FDMaturityAmount = grouped.Key.MaturityAmount,
                                        FDMaturityDate = grouped.Key.MaturityDate,
                                        FDIntAlreadyCalculated = grouped.Sum(g => g.trn.InterestCalculatedAmount),
                                        FDIntAlreadyCalculatedDate = grouped.Max(g => g.trn.InterestAppliedDate),
                                        FDIntAlreadyPaid = grouped.Sum(g => g.trn.InterestPaidAmount)
                                    }).FirstAsync();
                if (query != null && query.FDId >0) fdData = query;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching Term deposit payable data");
            }
            return fdData;
        }

        public async Task<List<DropdownItem>> GetTDNosByMemIdForRenewal(decimal memId, string tdSchemeType, DateTime trnDate, string brCode)
        {
            List<DropdownItem> fdList = new List<DropdownItem>();
            try
            {
                var query = await (from master in CSISContext.TermDeposit_Master
                                       // --- Join with Transactions (INNER JOIN) ---
                                   join trn in CSISContext.TermDeposit_Trn
                                   on master.TD_Id equals trn.TD_Id
                                   // --- Filtering (WHERE clauses combined) ---
                                   where master.TD_Delete == false &&
                                         master.AccountClosed == false &&
                                         trn.TD_Delete == false &&
                                         master.Mem_Id == memId &&
                                         master.MaturityDate <= trnDate.Date &&
                                         master.BrCode == brCode &&
                                         trn.BrCode == brCode
                                   // --- Grouping (GROUP BY) ---
                                   // Group the transaction by the master fields
                                   group trn by new { master.TD_Id, master.TD_No } into g // 'g' represents each group
                                                                                          // --- Aggregation Filtering (HAVING) ---
                                                                                          // Filter groups where the sum of DepositPaidAmount within the group is 0
                                   where g.Sum(t => t.DepositPaidAmount) == 0
                                   // --- Final Selection (SELECT) ---
                                   // Select the key of the group (which contains TD_Id and TD_No)
                                   select g.Key).ToListAsync();   // g.Key already holds the anonymous type { TD_Id, TD_No }
                if (query != null && query.Count > 0)
                {
                    foreach (var t in query)
                    {
                        DropdownItem item = new DropdownItem
                        {
                            Value = t.TD_Id.ToString(),
                            Text = t.TD_No,
                        };
                        fdList.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching Term deposit payable for renewal");
            }
            return fdList;
        }

        public async Task<DtoNominee> GetNomineeForTermDeposit(decimal memId, string tdSchemeType, string brCode)
        {
            DtoNominee nominee = new DtoNominee();
            try
            {
                var result = await (from td in CSISContext.TermDeposit_Master
                              join scheme in CSISContext.TermDeposit_Schemes
                                  on td.TDScheme_Id equals scheme.TDScheme_Id
                              where td.Mem_Id == memId &&
                                    td.BrCode == brCode &&
                                    td.TD_Delete == false &&
                                    td.Nominee1Name != null &&
                                    scheme.TDSchemeType == tdSchemeType
                              orderby td.TD_Id descending
                              select new DtoNominee
                              {
                                  Nominee1Name = td.Nominee1Name,
                                  Nominee1Age = td.Nominee1Age,
                                  Nominee1Relationship = td.Nominee1Relationship,
                                  Nominee2Name=td.Nominee2Name,
                                  Nominee2Age = td.Nominee2Age,
                                  Nominee2Relationship = td.Nominee2Relationship,
                              }).FirstOrDefaultAsync();

                if (result != null ) 
                    nominee = result;
                else
                {
                    var result1 = await (from td in CSISContext.TermDeposit_Master
                                         where td.Mem_Id == memId
                                         select new DtoNominee
                                         {
                                             Nominee1Name = td.Nominee1Name,
                                             Nominee1Age = td.Nominee1Age,
                                             Nominee1Relationship = td.Nominee1Relationship,
                                             Nominee2Name ="",
                                             Nominee2Age = 0,
                                             Nominee2Relationship ="",
                                         }).FirstOrDefaultAsync();
                    if(result1 != null ) nominee = result1;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching Term deposit nominee data");
            }
            return nominee;
        }

        public int Get_MaxTDTrnSlNo(decimal TDId)
        {
            int MaxSlNo = 0;
            try
            {
                var maxSlNo = CSISContext.TermDeposit_Trn.Where(x=> x.TD_Id == TDId).Max(x => x.Trn_SlNo);
                if (maxSlNo == 0) MaxSlNo = 1;
                else
                    MaxSlNo++;
            }
            catch (Exception)
            {
                MaxSlNo = 1;
            }
            int.TryParse(MaxSlNo.ToString(), out int result);
            return result;
        }

        #region security deposit
        public async Task<DtoSecurityDepositData> GetSecurityDepositData(decimal empId,string brCode)
        {
            DtoSecurityDepositData securityDeposit = new();
            try
            {
                var result = await (from trn in CSISContext.TermDeposit_Trn
                                    join master in CSISContext.TermDeposit_Master
                                        on trn.TD_Id equals master.TD_Id
                                    join emp in CSISContext.mem_master
                                        on master.Mem_Id equals emp.mem_id
                                    where master.Mem_Id == empId
                                          && master.TD_Delete == false
                                          && trn.TD_Delete == false
                                          && master.BrCode == brCode
                                          && trn.BrCode == brCode
                                          && emp.brcode == brCode
                                    group new { trn, master } by new { master.TD_Id, master.TD_No, emp.mem_id, emp.memberno, emp.membername } into g
                                    select new DtoSecurityDepositData
                                    {
                                        Employee_Id = g.Key.mem_id,
                                        Employee_No = g.Key.memberno,
                                        Employee_Name = g.Key.membername,
                                        Td_Id = g.Key.TD_Id,
                                        Td_No = g.Key.TD_No,
                                        Balance = g.Sum(x => x.trn.DepositReceiptAmount - x.trn.DepositPaidAmount)
                                    }).FirstOrDefaultAsync();
                if (result != null && result.Employee_Id > 0) securityDeposit = result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return securityDeposit;
        }
        #endregion 
    }
}
