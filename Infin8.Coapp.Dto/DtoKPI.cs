using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class DtoKPI
    {

    }
    public class KPINumeric
    {
        public decimal Target { get; set; }
        public decimal Achievement { get; set; }
        public decimal Percentage => Target > 0 ? (Achievement / Target) * 100 : 0;
        public string StatusColor => Percentage >= 100 ? "text-success" : Percentage >= 80 ? "text-warning" : "text-danger";
    }

    public class KPIOverdue
    {
        public decimal OverdueAmount { get; set; }
        public decimal TotalAmount { get; set; } // For calculating percentage
        public decimal OverduePercentage => TotalAmount > 0 ? (OverdueAmount / TotalAmount) * 100 : 0;
        public string StatusColor => OverduePercentage <= 2 ? "text-success" : OverduePercentage <= 5 ? "text-warning" : "text-danger";
    }

    public class DividendInfo
    {
        public int Year { get; set; }
        public decimal Percentage { get; set; }
    }

    // --- Entity Specific Models ---
    public class PCARDBankData
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public string DR_Circle_Id { get; set; } = "";

        // Frequently Reviewed
        public KPINumeric JewelLoanIssue { get; set; } = new();
        public KPIOverdue JewelLoanOverdue { get; set; } = new(); // Overdue %
        public int ActionsForOverdueJewelLoan { get; set; }
        public KPINumeric FixedDepositMobilization { get; set; } = new();
        public decimal CashOnHand { get; set; }

        // Monthly Reviewed
        public decimal JewelLoanBorrowings { get; set; }
        public KPIOverdue JewelLoanBorrowingsOverdue { get; set; } = new(); // Overdue %
        public KPIOverdue NonFarmSectorLoanOverdue { get; set; } = new(); // Overdue Amount & %

        // Annually Reviewed
        public int Members { get; set; }
        public decimal ShareCapitalOutstanding { get; set; }
        public List<DividendInfo> DividendHistory { get; set; } = new();
        public KPIOverdue NonPerformingAssets { get; set; } = new(); // NPA Amount & %
        public decimal ProfitOrLoss { get; set; } // Positive for Profit, Negative for Loss
        public string ProfitLossStatusColor => ProfitOrLoss >= 0 ? "text-success" : "text-danger";
    }

    public class DeputyRegistrarCircle
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public string JR_Region_Id { get; set; } = "";
        public List<PCARDBankData> Banks { get; set; } = new();

        // Aggregated KPIs (calculated from Banks)
        public KPINumeric TotalJewelLoanIssue => new KPINumeric { Target = Banks.Sum(b => b.JewelLoanIssue.Target), Achievement = Banks.Sum(b => b.JewelLoanIssue.Achievement) };
        public decimal AvgJewelLoanOverduePercentage => Banks.Any() ? Banks.Average(b => b.JewelLoanOverdue.OverduePercentage) : 0;
        // ... other aggregated KPIs
    }

    public class JointRegistrarRegion
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public List<DeputyRegistrarCircle> Circles { get; set; } = new();

        // Aggregated KPIs (calculated from Circles)
        public KPINumeric TotalJewelLoanIssue => new KPINumeric { Target = Circles.Sum(c => c.TotalJewelLoanIssue.Target), Achievement = Circles.Sum(c => c.TotalJewelLoanIssue.Achievement) };
        // ... other aggregated KPIs
    }

    public class TamilNaduStateData
    {
        public List<JointRegistrarRegion> Regions { get; set; } = new();
        // Aggregated KPIs (calculated from Regions)
        public KPINumeric TotalJewelLoanIssue => new KPINumeric { Target = Regions.Sum(r => r.TotalJewelLoanIssue.Target), Achievement = Regions.Sum(r => r.TotalJewelLoanIssue.Achievement) };
        // ... other aggregated KPIs
    }

}
