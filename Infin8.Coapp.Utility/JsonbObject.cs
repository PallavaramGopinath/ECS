using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infin8.Coapp.Utility
{
    public static class JsonbObject
    {
        #region Loan
        public static DtoJewelLoanRecovery ConvertFromJson(string jsonData)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // If you want case-insensitive property matching
            };

            return JsonSerializer.Deserialize<DtoJewelLoanRecovery>(jsonData, options)!;
        }

        public static DtoJewelLoanDisbursement ConvertFromJsonForJeweLoanDisbursement(string jsonData)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // If you want case-insensitive property matching
            };

            return JsonSerializer.Deserialize<DtoJewelLoanDisbursement>(jsonData, options)!;
        }

       
        #endregion 

        #region Fixed Deposit
        public static DtoTermDepositFixedDepositCreation ConvertFromJsonForNewFixedDeposit(string jsonData)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // If you want case-insensitive property matching
            };

            return JsonSerializer.Deserialize<DtoTermDepositFixedDepositCreation>(jsonData, options)!;
        }

        public static DtoTermDepositFixedDepositPayment ConvertFromJsonForNewFixedDepositPayment(string jsonData)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // If you want case-insensitive property matching
            };

            return JsonSerializer.Deserialize<DtoTermDepositFixedDepositPayment>(jsonData, options)!;
        }

        public static DtoTermDepositFixedDepositRenewal ConvertFromJsonForNewFixedDepositRenewal(string jsonData)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // If you want case-insensitive property matching
            };

            return JsonSerializer.Deserialize<DtoTermDepositFixedDepositRenewal>(jsonData, options)!;
        }
        public static DtoTermDepositFixedDepositRenewal ConvertFromJsonForRenewalFixedDepositPayment(string jsonData)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // If you want case-insensitive property matching
            };

            return JsonSerializer.Deserialize<DtoTermDepositFixedDepositRenewal>(jsonData, options)!;
        }

        public static DtoTermDepositFixedDepositLoan ConvertFromJsonForFixedDepositLoanDisbursement(string jsonData)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // If you want case-insensitive property matching
            };

            return JsonSerializer.Deserialize<DtoTermDepositFixedDepositLoan>(jsonData, options)!;
        }

        public static DtoTermDepositLoanRecovery ConvertFromJsonForFixedDepositLoanRecovery(string jsonData)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // If you want case-insensitive property matching
            };

            return JsonSerializer.Deserialize<DtoTermDepositLoanRecovery>(jsonData, options)!;
        }
        
        #endregion

        #region SB Account
        public static DtoSBAccountCreate ConvertFromJsonForNewSBAccount(string jsonData)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // If you want case-insensitive property matching
            };
            return JsonSerializer.Deserialize<DtoSBAccountCreate>(jsonData, options)!;
        }

        public static DtoSBAccountTransaction ConvertFromJsonForNewSBAccountTransaction(string jsonData)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // If you want case-insensitive property matching
            };
            return JsonSerializer.Deserialize<DtoSBAccountTransaction>(jsonData, options)!;
        }
        #endregion

        #region Jewel Loan
        public static DtoJewelLoanDisbursement ConvertFromJsonForJLDisbursement(string jsonData)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // If you want case-insensitive property matching
            };
            return JsonSerializer.Deserialize<DtoJewelLoanDisbursement>(jsonData, options)!;
        }
        #endregion

        #region Jewel Loan trn and voc trn
        public static List<Loan_Trn> GetJewelLoanRecovery(List<DtoJewelLoanBalance> jewelLoanBalances,
           DateTime transactionDate, decimal checkedBy, decimal vocId, decimal yrId, string brCode)
        {
            decimal loanId = 0;
            double piCalc = 0, piColl = 0, intCalc = 0, intColl = 0, prlColl = 0, roi = 0, piRate = 0;
            List<Loan_Trn> lnList = new();
            Loan_Trn ln = new();
            foreach (var jl in jewelLoanBalances)
            {
                if (loanId != jl.Loan_Id)
                {
                    if (loanId > 0)
                    {
                        ln = new();
                        ln = Utility.GetModalObject.GetLoanTrnObject(loanId, 0, "R", transactionDate, null, null, 0, 0, 0, piCalc, piCalc > 0 ? transactionDate : null, 0, null, intCalc, intCalc > 0 ? transactionDate : null, piColl, 0, intColl, prlColl, 0, 0, 0, null, 0, null, 0, null, roi, piRate, 0, 0, false, false, vocId,checkedBy, yrId, 0, false, 0, 0, 0, 0, 0, brCode);
                        lnList.Add(ln);
                        piCalc = 0; piColl = 0; intCalc = 0; intColl = 0; prlColl = 0;
                    }
                    loanId = jl.Loan_Id;
                    if (jl.Status == "Penal Interest")
                    {
                        piRate = jl.PI_Rate;
                        piCalc = jl.Calculated_Amount;
                        piColl = jl.Recovery_Amount;
                    }
                    if (jl.Status == "Interest")
                    {
                        roi = jl.ROI;
                        intCalc = jl.Calculated_Amount;
                        intColl = jl.Recovery_Amount;
                    }
                    if (jl.Status == "Principal")
                    {
                        prlColl = jl.Recovery_Amount;
                    }

                }
                else
                {
                    if (loanId == jl.Loan_Id)
                    {
                        if (jl.Status == "Penal Interest")
                        {
                            piRate = jl.PI_Rate;
                            piCalc = jl.Calculated_Amount;
                            piColl = jl.Recovery_Amount;
                        }
                        if (jl.Status == "Interest")
                        {
                            roi = jl.ROI;
                            intCalc = jl.Calculated_Amount;
                            intColl = jl.Recovery_Amount;
                        }
                        if (jl.Status == "Principal")
                        {
                            prlColl = jl.Recovery_Amount;
                        }
                    }
                }
            }
            if (loanId > 0)
            {
                ln = new();
                ln = Utility.GetModalObject.GetLoanTrnObject(loanId, 0, "R", transactionDate, null, null, 0, 0, 0, piCalc,
                    piCalc > 0 ? transactionDate : null, 0, null, intCalc, intCalc > 0 ? transactionDate : null, piColl, 0, intColl, prlColl, 0, 0, 0, null, 0, null, 0, null, roi, piRate, 0, 0, false, false, vocId,checkedBy, yrId, 0,  false, 0, 0, 0, 0, 0, brCode);
                lnList.Add(ln);
            }
            return lnList;
        }
        public static List<Fin_Voucher_Trn> GetVoucherTrnForJewelLoanRecovery(List<DtoJewelLoanBalance> jewelLoanBalances, decimal vocId, int trnType,
            decimal memberId, string memberNo, string memberName, decimal usrId, decimal yrId, string status, string brCode)
        {
            double overdue = 0;
            double balance = 0;
            List<Fin_Voucher_Trn> finVocList = new();
            string narration = memberNo.Trim() + " " + memberName.Trim();
            foreach (var jl in jewelLoanBalances)
            {
                if (jl.Status == "Penal Interest")
                {
                    overdue = jl.Outstanding - jl.Calculated_Amount;
                    balance = jl.Outstanding - jl.Recovery_Amount;
                    Fin_Voucher_Trn voc = Utility.GetModalObject.GetFinVoucherTrObject(vocId, jl.Led_Id, jl.Recovery_Amount, 0, trnType, narration + " Loan No: " + jl.Loan_No!.Trim(), false, usrId, yrId, status,"Loan No : " + jl.Loan_No, memberId, brCode, jl.Loan_Id, jl.Outstanding, balance);
                    finVocList.Add(voc);
                }
                if (jl.Status == "Interest")
                {
                    overdue = jl.Outstanding - jl.Calculated_Amount;
                    balance = jl.Outstanding - jl.Recovery_Amount;
                    Fin_Voucher_Trn voc = Utility.GetModalObject.GetFinVoucherTrObject(vocId, jl.Led_Id, jl.Recovery_Amount, 0, trnType, narration + " Loan No: " + jl.Loan_No!.Trim(), false, usrId, yrId, status, "Loan No : " + jl.Loan_No, memberId, brCode, jl.Loan_Id, jl.Outstanding, balance);
                    finVocList.Add(voc);
                }
                if (jl.Status == "Principal")
                {
                    overdue = jl.Outstanding - jl.Calculated_Amount;
                    balance = jl.Outstanding - jl.Recovery_Amount;
                    Fin_Voucher_Trn voc = Utility.GetModalObject.GetFinVoucherTrObject(vocId, jl.Led_Id, jl.Recovery_Amount, 0, trnType, narration + " Loan No: " + jl.Loan_No!.Trim(), false, usrId, yrId, status, "Loan No : " + jl.Loan_No, memberId, brCode, jl.Loan_Id, jl.Outstanding, balance);
                    finVocList.Add(voc);
                }
            }
            return finVocList;
        }
        #endregion

        #region other related data
        public static DtoOtherRelatedData ConvertFromJsonForOtherRelatedData(string jsonData)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // If you want case-insensitive property matching
            };

            return JsonSerializer.Deserialize<DtoOtherRelatedData>(jsonData, options)!;
        }
        #endregion

        #region Account Transaction
        public static DtoAccountTransactionRelatedData ConvertFromJsonForAccountTransactionRelatedData(string jsonData)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // If you want case-insensitive property matching
            };

            return JsonSerializer.Deserialize<DtoAccountTransactionRelatedData>(jsonData, options)!;
        }
        #endregion 
    }
}
