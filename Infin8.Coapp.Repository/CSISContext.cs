using Microsoft.EntityFrameworkCore;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore.Metadata;
using Models;

namespace Infin8.Coapp.Repository
{
    public partial class CSISContext :DbContext
    {
        public CSISContext(DbContextOptions<CSISContext> options) :base(options)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            if (Database.IsNpgsql()) // Check if the database is PostgreSQL
            {
                modelBuilder.UseLowerCaseTableAndColumnNames();
            }
        }

        public virtual DbSet<Account_Transactions> Account_Transactions { get; set; }

        public virtual DbSet<Business_Day> Business_Day { get; set; }
        #region Employee
        public virtual DbSet<Emp_Job_History> Emp_Job_History {  get; set; }
        public virtual DbSet<Emp_Master> Emp_Master { get; set; }
        public virtual DbSet<Emp_Pf> Emp_Pf { get; set; }
        public virtual DbSet<Emp_PFCalcCalendar> Emp_PFCalcCalendar { get; set; }
        public virtual DbSet<Emp_Qualification> Emp_Qualification { get; set; }
        #endregion

        #region Gen_Bank_Name
        public virtual DbSet<Gen_Bank_Name> Gen_Bank_Name { get; set; }
        #endregion 

        #region Member
        public virtual DbSet<mem_master> mem_master { get; set; }
        public virtual DbSet<Mem_Trn> Mem_Trn { get; set; }
        public virtual DbSet<Mem_Address> Mem_Address { get; set; }
        #endregion

        #region Financial Accounts
        public virtual DbSet<Fin_Ledger> Fin_Ledger { get; set; }
        public virtual DbSet<Fin_Ledger_Fnl> Fin_Ledger_Fnl { get; set; }
        public virtual DbSet<Fin_Ledger_Grp> Fin_Ledger_Grp { get; set; }
        public virtual DbSet<Fin_Ledger_SubGrp>Fin_Ledger_SubGrp { get; set; }
        public virtual DbSet<Fin_Ledger_Trn>Fin_Ledger_Trn { get; set; }
        public virtual DbSet<Fin_Voucher>Fin_Voucher { get; set; }
        public virtual DbSet<Fin_Voucher_Trn>Fin_Voucher_Trn { get; set; } 
        public virtual DbSet<Fin_Voucher_Bank>Fin_Voucher_Bank { get; set; }
        public virtual DbSet<Fin_Voucher_Bank_Trn>Fin_Voucher_Bank_Trn { get; set; }
        public virtual DbSet<Fin_Yr_Master>Fin_Yr_Master { get; set; }
        #endregion 

        #region Refer
        public virtual DbSet<Refer_Data> Refer_Data { get; set; }
        public virtual DbSet<Refer_Area> Refer_Area { get; set; }
        public virtual DbSet<Refer_District> Refer_District { get; set; }
        public virtual DbSet<Bank_Master> Bank_Master { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<Refer_Constituency> Refer_Constituency { get; set; }
        #endregion 

        #region loan
        public virtual DbSet<Loan_Master> Loan_Master { get; set; }
        public virtual DbSet<Loan_Disb> Loan_Disb {  get; set; }
        public virtual DbSet<Loan_Inst> Loan_Inst { get; set; }
        public virtual DbSet<Loan_Roi> Loan_Roi { get; set; }
        public virtual DbSet<Loan_Roi_Template> Loan_Roi_Template { get; set; }
        public virtual DbSet<Loan_Members>  Loan_Members { get; set; }
        public virtual DbSet<Loan_Trn> Loan_Trn { get; set; }
        public virtual DbSet<Loan_Schemes> Loan_Schemes { get; set; }
        public virtual DbSet<Loan_Schemes_Group> Loan_Schemes_Group { get; set; }
        public virtual DbSet<Loan_Sanction> Loan_Sanction { get; set; }
        public virtual DbSet<Loan_Sanction_Trn> Loan_Sanction_Trn { get; set; }
        public virtual DbSet<Loan_Repayment_Schedule> Loan_Repayment_Schedule { get; set; }
        public virtual DbSet<Lien> Lien { get; set; }
        public virtual DbSet<Lien_Trn> Lien_Trn { get; set; }
        public virtual DbSet<JL_DailyMarketRate> JL_DailyMarketRate { get; set; }
        public virtual DbSet<JL_Details> JL_Details { get; set; }
        public virtual DbSet<JL_LoanEligible> JL_LoanEligible { get; set; }
        public virtual DbSet<JL_Max_Limit> JL_Max_Limit { get; set; }
        public virtual DbSet<JL_Ornments> JL_Ornments { get; set; }
        public virtual DbSet<JL_Processingfees> JL_ProcessingFees { get; set; }

        #endregion

        #region Map
        public virtual DbSet<Map_Banks> Map_Banks { get; set; }
        public virtual DbSet<Map_General> Map_General { get; set; }
        public virtual DbSet<Map_SuspenseAccounts> Map_SuspenseAccounts { get; set; }
        #endregion  

        #region Demand
        public virtual DbSet<Mem_Demand_Master> Mem_Demand_Master { get; set; }
        #endregion
        public virtual DbSet<Mem_PassBook> Mem_PassBook { get; set; }

        #region Mem Payable
        public virtual DbSet<Mem_Payable>Mem_Payable { get; set; } 
        public virtual DbSet<Mem_Payable_Master> Mem_Payable_Master { get; set; } 
        public virtual DbSet<Mem_Payable_NotNEFT> Mem_Payable_NotNEFT { get; set; }
        #endregion

        #region Pay
        public virtual DbSet<Pay_Components> Pay_Components { get; set; }
        public virtual DbSet<Pay_Component_Assignments> Pay_Component_Assignments { get; set; }
        public virtual DbSet<Pay_All_Ded> Pay_All_Ded { get; set; }
        public virtual DbSet<Pay_All_Ded_Master> Pay_All_Ded_Master { get; set; }
        public virtual DbSet<Pay_Att> Pay_Att { get; set; }
        public virtual DbSet<Pay_BasicPay> Pay_BasicPay { get; set; }
        public virtual DbSet<Pay_DA_Template> Pay_DA_Template { get; set; }
        public virtual DbSet<Pay_Gen_Info> Pay_Gen_Info { get; set; }
        public virtual DbSet<Pay_Init>Pay_Init { get; set; }
        public virtual DbSet<Pay_Leave_Eligible> Pay_Leave_Eligible { get; set; }
        public virtual DbSet<Pay_PF_ROITemplate> Pay_PF_ROITemplate { get; set; }
        public virtual DbSet<Pay_PF_Template> Pay_PF_Template { get; set; }
        public virtual DbSet<Pay_Retirement> Pay_Retirement { get; set; }
        public virtual DbSet<Pay_Slip>Pay_Slip { get; set; }
        public virtual DbSet<Pay_Slip_Loan_Trn> Pay_Slip_Loan_Trn { get; set; }
        public virtual DbSet<Pay_Slip_Trn> Pay_Slip_Trn { get; set; }
        public virtual DbSet<Pay_Template> Pay_Template { get; set; }
        public virtual DbSet<Pay_VPF> Pay_VPF { get; set; }

        #endregion

        #region Report master
        public virtual DbSet<Reports_Group> Reports_Group { get; set; }
        public virtual DbSet<Reports_Master> Reports_Master { get; set; }
        #endregion 

        #region Term Deposit
        public virtual DbSet<TermDeposit_FcTemplate> TermDeposit_FcTemplate { get; set; }
        public virtual DbSet<TermDeposit_IntCalc_Calendar>TermDeposit_IntCalc_Calender { get; set; }
        public virtual DbSet<TermDeposit_LoanEligibleTemplate>TermDeposit_LoanEligibleTemplate { get; set; }
        public virtual DbSet<TermDeposit_Master> TermDeposit_Master { get; set; }
        public virtual DbSet<TermDeposit_Members>TermDeposit_Members { get; set; }
        public virtual DbSet<TermDeposit_Roi_Template>TermDeposit_Roi_Template { get; set; }   
        public virtual DbSet<TermDeposit_Schemes>TermDeposit_Schemes { get; set; }
        public virtual DbSet<TermDeposit_Trn>TermDeposit_Trn { get; set; }
        #endregion

        #region SB 
        public virtual DbSet<SBCA_Master>SBCA_Master { get; set; } 
        public virtual DbSet<SBCA_Schemes> SBCA_Schemes { get; set; }
        #endregion

        #region society options and neft
        public virtual DbSet<Society_NEFT> Society_NEFT { get; set; }
        public virtual DbSet<Society_Options> Society_Options { get; set; }
        #endregion 

        #region Refresh Token 
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
        #endregion

        #region Staging
        public virtual DbSet<Staging_Master> Staging_Master { get; set; }
        public virtual DbSet<Staging_Details> Staging_Details { get; set; }
        public virtual DbSet<Staging_History> Staging_History { get; set; }
        public virtual DbSet<Staging_Balance> Staging_Balance { get; set; }
        #endregion

        #region Menu
        public virtual DbSet<Menu_Main> Menu_Main { get; set; }
        public virtual DbSet<Menu_Sub> Menu_Sub { get; set; }
        public virtual DbSet<Menu_Forms> Menu_Forms { get; set; }
        #endregion 
    }

    public static class ModelBuilderExtensions
    {
        public static void UseLowerCaseTableAndColumnNames(this ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Set table name to lowercase
                entity.SetTableName(entity.GetTableName()?.ToLower());

                // Set column names to lowercase
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.GetColumnName(StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema()))?.ToLower());
                }

                // Set foreign key names to lowercase
                foreach (var key in entity.GetKeys())
                {
                    key.SetName(key.GetName()?.ToLower());
                }

                foreach (var fk in entity.GetForeignKeys())
                {
                    fk.SetConstraintName(fk.GetConstraintName()?.ToLower());
                }
            }
        }
    }

}
