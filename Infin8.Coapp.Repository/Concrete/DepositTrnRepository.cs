using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Text;

namespace Infin8.Coapp.Repository
{
    public  class DepositTrnRepository(DbContext context) : Repository<DepositTrnRepository>(context), IDepositTrnRepositoty
    {
        public CSISContext CSISContext => (CSISContext)Context;

        public Task<(bool result, decimal depositTrnId, string depositTrnNo)> AddDepositTrnAsync(Deposit_Trn depositTrn)
        {
            throw new NotImplementedException();
        }
        public Task<bool> EditDepositTrnAsync(Deposit_Trn depositTrn)
        {
            throw new NotImplementedException();
        }
        public Task<List<Deposit_Trn>> GetDepositTrns(decimal memId, string brCode)
        {
            throw new NotImplementedException();
        }

        //public Task<List<Mem_Demand>> GetDepositDemand(DateTime dueDate, string brCode)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<List<Mem_Demand>> CalculateDepositDemand( decimal memId, DateOnly demandCalcDate, DateOnly demandDate, double memBasicPay, string brCode)
        {
            double _piArrear = 0, _piCurrent = 0, _depArrear = 0, _depCurrent = 0;
            List<Mem_Demand> depositDemand = [];
            List<Deposit_Masters> depMaster = [];
            List<DepositBalanceVM> depositBalanceList = [];
            try
            {

                //depMaster = [.. CSISContext.Deposit_Masters.Where(x => x.Deposit_Id <=3  && x.Is_Active == true && x.BrCode == brCode)];
                
                var balances = await  (from trn in CSISContext.Deposit_Trn
                                  join mas in CSISContext.Deposit_Masters on trn.Deposit_Id equals mas.Deposit_Id
                                  where trn.Mem_Id == memId && trn.Is_Active == true && trn.BrCode == brCode && mas.Deposit_Id <= 3
                                  group new { trn, mas } by new
                                  {
                                      trn.Mem_Id,
                                      trn.Deposit_Id,
                                      mas.Deposit_Name,
                                      mas.Led_Id,
                                      mas.Interest_Led_id,
                                      mas.PI_Led_Id,
                                      mas.Is_Arrear_Demand_Applicable
                                  } into g
                                  select new DepositBalanceVM
                                  {
                                      MemId = (int)g.Key.Mem_Id,
                                      DMId = g.Key.Deposit_Id,
                                      TrnDate = g.Max(x => x.trn.Trn_Date),
                                      DemandAmount = g.Sum(x => (double)x.trn.Demand_Amount),
                                      ReceiptAmount = g.Sum(x => (double)x.trn.Receipt_Amount),
                                      PaidAmount = g.Sum(x => (double)x.trn.Paid_Amount),
                                      IntAlreadyCalculated = g.Sum(x => (double)x.trn.Interest_Calculated_Amount),
                                      IntPaidAmount = g.Sum(x => (double)x.trn.Interest_Paid_Amount),
                                      PIAlreadyCalculated = g.Sum(x => (double)x.trn.Penal_Interest_Calculated_Amount),
                                      PIAlreadyCalculatedDate = g.Max(x => x.trn.Penal_Interest_Calculated_Date),
                                      PICollected = g.Sum(x => (double)x.trn.Penal_Interest_Received_Amount),
                                      DMName = g.Key.Deposit_Name,
                                      PrlLedId = (int)g.Key.Led_Id,
                                      IntLedId = (int)g.Key.Interest_Led_id,
                                      PILedId = (int)g.Key.PI_Led_Id,
                                      ArrearDemandApplication = (int)g.Key.Is_Arrear_Demand_Applicable
                                  }).ToListAsync();
                if(balances != null && balances.Count >0)
                {
                    depositBalanceList = balances;
                }

                foreach(var dep in depositBalanceList)
                {
                    /// Calcualte currend demand
                    _depCurrent = Calculate_Current_Deposit_Demand(dep.MemId, dep.DMId, demandDate, memBasicPay);

                }


                if (depMaster.Count > 0)
                {
                    foreach (var master in depMaster)
                    {
                        //_piArrear = 0; _piCurrent = 0; _depArrear = 0; _depCurrent = 0;
                        Mem_Demand single = new()
                        {
                            Id = 0,
                            Demand_Id = 0,
                            Calculated_Date = demandCalcDate,
                            Recovery_Date = null,
                            Demand_Type = "D",
                            Mem_Id = memId,
                            Deposit_Id = master.Deposit_Id
                        };
                        /// get arrear

                        DepositBalanceVM depBal = new();
                        
                        if (master.Is_Arrear_Demand_Applicable == 1)
                        {
                            var depBalance = (from trn in CSISContext.Deposit_Trn
                                          join mas in CSISContext.Deposit_Masters on trn.Deposit_Id equals mas.Deposit_Id
                                          where trn.Mem_Id == memId && trn.Is_Active == true && trn.Deposit_Id == master.Deposit_Id && trn.Mem_Id == master.Mem_Id && trn.BrCode == brCode
                                              group new { trn, mas } by new
                                          {
                                              trn.Mem_Id,
                                              trn.Deposit_Id,
                                              mas.Deposit_Name,
                                              mas.Led_Id,
                                              mas.Interest_Led_id,
                                              mas.PI_Led_Id,
                                              mas.Is_Arrear_Demand_Applicable
                                          } into g
                                          select new DepositBalanceVM
                                          {
                                              MemId = (int)g.Key.Mem_Id,
                                              DMId = g.Key.Deposit_Id,
                                              TrnDate = g.Max(x => x.trn.Trn_Date),
                                              DemandAmount = g.Sum(x => (double)x.trn.Demand_Amount),
                                              ReceiptAmount = g.Sum(x => (double)x.trn.Receipt_Amount),
                                              PaidAmount = g.Sum(x => (double)x.trn.Paid_Amount),
                                              IntAlreadyCalculated = g.Sum(x => (double)x.trn.Interest_Calculated_Amount),
                                              IntPaidAmount = g.Sum(x => (double)x.trn.Interest_Paid_Amount),
                                              PIAlreadyCalculated = g.Sum(x => (double)x.trn.Penal_Interest_Calculated_Amount),
                                              PIAlreadyCalculatedDate = g.Max(x => x.trn.Penal_Interest_Calculated_Date ),
                                              PICollected = g.Sum(x => (double)x.trn.Penal_Interest_Received_Amount),
                                              DMName = g.Key.Deposit_Name,
                                              PrlLedId = (int)g.Key.Led_Id,
                                              IntLedId = (int)g.Key.Interest_Led_id,
                                              PILedId = (int)g.Key.PI_Led_Id,
                                              ArrearDemandApplication = (int)g.Key.Is_Arrear_Demand_Applicable
                                          }).FirstOrDefault();

                            if(depBalance != null && depBalance.DMId >0) depBal = depBalance;
                            
                            if(depBalance != null && depBalance)



                            if (depBal != null)
                            {
                                _depArrear = depBal.DemandAmount - depBal.ReceiptAmount > 0 ? Math.Round(depBal.DemandAmount - depBal.ReceiptAmount, 2) : 0;
                                if (depBal.PIAlreadyCalculated != null)
                                {
                                    _piArrear = (double)depBal.PIAlreadyCalculated - depBal.PICollected > 0 ? Math.Round((double)depBal.PIAlreadyCalculated - depBal.PICollected, 2) : 0;
                                }
                            }
                            else
                            {
                                _depArrear = 0;
                                _piArrear = 0;
                            }
                            /// Calculate Penal Interest for FWD

                            var PIRate = CSISContext.Deposit_Roi_Templates
                                .Where(x => x.Deposit_Id == master.Deposit_Id && x.Penal_Interest_Rate > 0 && x.Wef <= demandDate)
                                .OrderByDescending(x => x.Wef)
                                .Select(x => x.Penal_Interest_Rate)
                                .FirstOrDefault();
                            //PIRate = roi > 0 ? roi : 0;
                            if (PIRate > 0)
                            {
                                if (_depArrear > 0)
                                    _piCurrent = (double)((_depArrear * (PIRate / 100) * 1 / 12) + 0.5);
                                else
                                    _piCurrent = 0;
                            }
                        }

                        /// Calcualte currend demand
                        _depCurrent = Calculate_Current_Deposit_Demand(memId, master.Deposit_Id, demandDate, memBasicPay);
                        
                        /// Calculate SRF demand
                        if (master.Frequency == "A")
                        {
                            if (master.Deposit_Id == 3)
                            {
                                if (demandDate.Month != master.Demand_Month)
                                {
                                    _depCurrent = 0;
                                }
                            }
                        }
                        single.Deposit_PI_Arrear = _depArrear;
                        single.Deposit_Current  = _depCurrent;
                        single.Deposit_PI_Arrear  = _piArrear;
                        single.Deposit_PI_Current  = _piCurrent;
                        single.Deposit_Total = _depArrear + _depCurrent + _piArrear + _piCurrent;
                        single.Total_Demand = single.Deposit_Total;

                        single.Recovery_Id = 0;
                        single.Usr_Id = 0;
                        single.Yr_Id = 0;
                        single.Voc_Id = 0;
                        if (single.Total_Demand > 0)
                            depositDemand.Add(single);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " " + ex.StackTrace);
            }

            return depositDemand;
        }

        public double Calculate_Current_Deposit_Demand(decimal Memid, int DMId, DateOnly DemandCalcDate, double memBasicPay)
        {
            double currentDepositDemand = 0;
            double optionAmount = 0;
            try
            {
                optionAmount = CSISContext.Deposit_Options
                    .Where(x => x.Mem_Id == Memid && x.Deposit_Id == DMId
                                && x.Is_Active == true
                                && x.Wef <= DemandCalcDate.ToDateTime(TimeOnly.MinValue))
                    .OrderByDescending(x => x.Wef)
                    .Select(x => x.Option_Amount)
                    .FirstOrDefault();
               
                var depositMaster = CSISContext.Deposit_Masters.Where(x => x.Deposit_Id == DMId).FirstOrDefault(); ///  dbDeposit.GetDepositMasterDetails(DMId, out errorMessage);
                
                if (depositMaster!.Percentage > 0)
                    currentDepositDemand = Math.Round(memBasicPay * (depositMaster.Percentage / 100), 0);
                else
                    currentDepositDemand = depositMaster.Fixed_Amount;
                if (optionAmount > 0)
                    currentDepositDemand = optionAmount;
                if (depositMaster.Frequency == "A")
                {
                    if (DemandCalcDate.Month != depositMaster.Demand_Month)
                        currentDepositDemand = 0;
                }
            }
            catch (Exception)
            {
                currentDepositDemand = 0;
            }
            return currentDepositDemand;
        }
    }
}
