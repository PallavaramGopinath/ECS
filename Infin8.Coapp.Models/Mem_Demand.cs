using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{
    using System;
    using System.ComponentModel;

    public partial class Mem_Demand
    {
        [Key]
        public decimal Id { get; set; }
        public decimal Demand_Id { get; set; }
        public  DateOnly Calculated_Date { get; set; }
        public DateOnly? Recovery_Date { get; set; }
        public string? Demand_Type { get; set; }
        public Decimal Mem_Id { get; set; }

        /// <summary>
        /// Loan
        /// </summary>
        public Decimal Loan_Id { get; set; }
        public double Prl_Schedule { get; set; }
        public double Non_OD_Prl { get; set; }
        public double Loan_Oustanding { get; set; }
        public double Loan_PI_Arrear { get; set; }
        public double Loan_Int_Arrear { get; set; }
        public double Loan_Prl_Arrear { get; set; }
        public double Loan_PI_Current { get; set; }
        public double Loan_Int_Current { get; set; }
        public double Loan_Prl_Current { get; set; }
        public double Loan_Total { get; set; }

        /// <summary>
        /// Deposits
        /// </summary>
        public int Deposit_Id { get; set; }
        public double Deposit_Arrear { get; set; }
        public double Deposit_Current { get; set; }
        public double Deposit_PI_Arrear { get; set; }
        public double Deposit_PI_Current { get; set; }
        public double Deposit_Total { get; set; }

        /// <summary>
        /// Sundry Debtors
        /// </summary>
        public decimal Debtor_Led_Id { get; set; }
        public double Debtor_Arrear { get; set; }
        public double Debtor_Current { get; set; }
        public double Debtor_Total { get; set; }

        /// <summary>
        /// Recurring Deposits
        /// </summary>
        public decimal TD_Id { get; set; }
        public int RD_Instalment { get; set; }
        public int RD_NoOf_Instalments { get; set; }
        public double RD_Demand { get; set; }
        public double RD_PI_Arrear { get; set; }
        public double RD_PI_Calc { get; set; }
        public DateTime? RD_PI_Application_Date { get; set; }
        public double RD_Total { get; set; }

        /// <summary>
        /// Due By Demand (CVRDE group insurance demand)
        /// </summary>
        public decimal DueBy_Led_Id { get; set; }
        public double DueBy_Arrear { get; set; }
        public double DueBy_Current { get; set; }
        public double DueBy_Total { get; set; }

        /// <summary>
        /// Total Demand
        /// </summary>
        public double Total_Demand { get; set; }


        /// <summary>
        /// Loan Collection
        /// </summary>
        public double loan_PI_Arrear_Collection { get; set; }
        public double Loan_Int_Arrear_Collection { get; set; }
        public double Loan_Prl_Arrear_Collection { get; set; }
        public double Loan_PI_Current_Collection { get; set; }
        public double Loan_Int_Current_Collection { get; set; }
        public double Loan_Prl_Current_Collection { get; set; }
        public double Loan_Total_Collection { get; set; }

        /// <summary>
        /// Deposit Collection
        /// </summary>
        public double Deposit_Arrear_Collection { get; set; }
        public double Deposit_Current_Collection { get; set; }
        public double Deposit_Arrear_PI_Collection { get; set; }
        public double Deposit_Current_PI_Collection { get; set; }
        public double Deposit_Total_Collection { get; set; }

        /// <summary>
        /// Sundry Debtors Collection
        /// </summary>
        public double Debtor_Arrear_Collection { get; set; }
        public double Debtor_Current_Collection { get; set; }
        public double Debtor_Total_Collection { get; set; }

        /// <summary>
        /// Recurring Deposits Collection
        /// </summary>
        public double RD_Receipt { get; set; }
        public double RD_PI_Receipt { get; set; }
        public double RD_Total_Receipt { get; set; }

        /// <summary>
        /// Advance Collection(Sundry Creditors)
        /// </summary>
        public decimal Creditor_Led_Id { get; set; }
        public double Creditor_Collection { get; set; }

        /// <summary>
        /// Due By Collection (CVRDE group insurance collection)
        /// </summary>
        public double DueBy_Arrear_Collection { get; set; }
        public double DueBy_Current_Collection { get; set; }
        public double DueBy_Total_Collection { get; set; }

        /// <summary>
        /// Total collection
        /// </summary>
        public double Total_Collection { get; set; }

        /// <summary>
        /// General
        /// </summary>
        public decimal Voc_Id { get; set; }
        public decimal Usr_Id { get; set; }
        public decimal Yr_Id { get; set; }
        public string? BrCode { get; set; }
        public bool Is_Active { get; set; } = true;
        public bool Stop_Demand { get; set; } =false;
        public bool Is_Appropriated { get; set; }=false;
        public bool Is_Recovery_Saved { get; set; } = false;
        public decimal Recovery_Id { get; set; }

        /// <summary>
        /// Loan2 Recalculation
        /// </summary>
        public double Loan_PI_Arrear2 { get; set; }
        public double Loan_Int_Arrear2 { get; set; }
        public double Loan_Prl_Arrear2 { get; set; }
        public double Loan_PI_Current2 { get; set; }
        public double Loan_Int_Current2 { get; set; }
        public double Loan_Prl_Current2 { get; set; }
        public double Loan_Total2 { get; set; }

        /// <summary>
        /// Deposits2
        /// </summary>
        public double Deposit_Arrear2 { get; set; }
        public double Deposit_Current2 { get; set; }
        public double Deposit_PI_Arrear2 { get; set; }
        public double Deposit_PI_Current2 { get; set; }
        public double Deposit_Total2 { get; set; }

        /// <summary>
        /// Sundry Debtors2
        /// </summary>
        public double Debtor_Arrear2 { get; set; }
        public double Debtor_Current2 { get; set; }
        public double Debtor_Total2 { get; set; }

        /// <summary>
        /// Recurring Deposits2
        /// </summary>
        public int RD_Instalment2 { get; set; }
        public int RD_NoOf_Instalments2 { get; set; }
        public double RD_Demand2 { get; set; }
        public double RD_PI_Arrear2 { get; set; }
        public double RD_PI_Calc2 { get; set; }
        public DateTime? RD_PI_Application_Date2 { get; set; }
        public double RD_Total2 { get; set; }

        /// <summary>
        /// Due By Demand2 (CVRDE group insurance demand)
        /// </summary>
        public double DueBy_Arrear2 { get; set; }
        public double DueBy_Current2 { get; set; }
        public double DueBy_Total2 { get; set; }

        /// <summary>
        /// Total Demand2
        /// </summary>
        public double Total_Demand2 { get; set; }
            }
}
