using System;
using System.Collections.Generic;

namespace Infin8.Coapp.Dto
{
    /// <summary>
    /// One grid row of the ECS Loan Recovery component (spec 11 §7, §10). All balances are as on the
    /// application date (the business day); "Now" figures are the accrual from the last calculation date to
    /// application date − 1.
    /// </summary>
    public class EcsLoanRecoveryLoanVM
    {
        public decimal Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public int Scheme_Id { get; set; }
        public string? Scheme_Name { get; set; }
        public DateTime? Disb_Date { get; set; }
        public double Roi { get; set; }
        public double Pi { get; set; }

        // scheme rule columns + ledgers (needed for posting; harmless on the client)
        public int Inst_Type { get; set; }
        public int Int_Application { get; set; }
        public int PI_Application { get; set; }
        public decimal PrlLed_Id { get; set; }
        public decimal IntLed_Id { get; set; }
        public decimal PILed_Id { get; set; }

        // §4.1 balances
        public double TotalDisbursed { get; set; }
        public double TotalPrlDemand { get; set; }
        public double TotalPrlCollection { get; set; }
        public double PrlOutstanding { get; set; }
        public double PrlOverdue { get; set; }
        public double IntOverdue { get; set; }
        public double PIOverdue { get; set; }
        public DateTime? LastIntDate { get; set; }
        public DateTime? LastPIDate { get; set; }

        // §6 accrual now
        public double IntNow { get; set; }
        public DateTime? IntFrom { get; set; }
        public DateTime? IntTo { get; set; }
        public int IntDays { get; set; }
        public double PINow { get; set; }
        public DateTime? PIFrom { get; set; }
        public DateTime? PITo { get; set; }
        public int PIDays { get; set; }

        /// <summary>Rate in force on the application date (stamped on the posted loan_trn row).</summary>
        public double CurrentRoi { get; set; }
        public double CurrentPi { get; set; }

        public double Recoverable { get; set; }
        public double ToClose { get; set; }

        /// <summary>Loan already sits in another pending (I/M) acc 58 staging line.</summary>
        public bool IsPending { get; set; }
    }

    public class EcsLoanRecoveryPreviewVM
    {
        public DateTime ApplicationDate { get; set; }
        public List<EcsLoanRecoveryLoanVM> Loans { get; set; } = new();
        public string? Message { get; set; }
    }

    /// <summary>Appropriation result for one selected loan (spec 11 §8).</summary>
    public class EcsAppropriationLineVM
    {
        public decimal Loan_Id { get; set; }
        public double PIColl { get; set; }
        public double IntColl { get; set; }
        public double PrlColl { get; set; }
        public double PrlOsAfter { get; set; }
        public bool ClosesLoan { get; set; }
    }

    /// <summary>The <c>Related_Account_Data</c> JSON of an acc 58 staging line (spec 11 §10).</summary>
    public class DtoEcsLoanRecovery
    {
        public decimal MemId { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public DateTime ApplicationDate { get; set; }
        public double RecoveryAmount { get; set; }
        public int CashOrAdjustment { get; set; }
        public List<DtoEcsLoanRecoveryLine> Lines { get; set; } = new();
    }

    public class DtoEcsLoanRecoveryLine
    {
        public decimal Loan_Id { get; set; }
        public string? Loan_No { get; set; }
        public string? Scheme_Name { get; set; }
        public double IntNow { get; set; }
        public DateTime? IntCalcDate { get; set; }
        public double PINow { get; set; }
        /// <summary>Null when the scheme's pi_application = 1 (no penal).</summary>
        public DateTime? PICalcDate { get; set; }
        public double PIColl { get; set; }
        public double IntColl { get; set; }
        public double PrlColl { get; set; }
        public double Soc_Roi { get; set; }
        public double SocPI_Rate { get; set; }
        public bool ClosesLoan { get; set; }
    }
}
