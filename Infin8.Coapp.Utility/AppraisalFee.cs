using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infin8.Coapp.Dto;
namespace Infin8.Coapp.Utility
{
    public static class AppraisalFee
    {
        public static DtoAppraisalFee GetAppraisalFee(double loanAmount)
        {
            DtoAppraisalFee app = new DtoAppraisalFee();
            double totalFee = 0;
            double appraisalFee = 0;
            double bankCharges = 0;
            try
            {
                switch(loanAmount )
                {
                    case <= 50000:
                        totalFee = Math.Round(loanAmount * 0.003,0);
                        if(totalFee < 100 ) totalFee = 100;
                        if (totalFee > 150) totalFee = 150;
                        break;
                    case >= 50001 and <= 200000:
                        totalFee = Math.Round(loanAmount * 0.002, 0);
                        if (totalFee < 150) totalFee = 150;
                        if (totalFee > 400) totalFee = 400;
                        break;
                    case >= 200001 and <= 500000:
                        totalFee = Math.Round(loanAmount * 0.002, 0);
                        if (totalFee < 400) totalFee = 400;
                        if (totalFee > 500) totalFee = 500;
                        break;
                    case >= 500001 and <= 1000000:
                        totalFee = Math.Round(loanAmount * 0.001, 0);
                        if (totalFee < 500) totalFee = 500;
                        if (totalFee > 1000) totalFee = 1000;
                        break;
                    case >= 1000001:
                        totalFee = Math.Round(loanAmount * 0.001, 0);
                        if (totalFee < 1000) totalFee = 1000;
                        if (totalFee > 1500) totalFee = 1500;
                        break;
                }
                bankCharges = Math.Round(totalFee * 0.2,0);
                appraisalFee = totalFee - bankCharges;
                app.AppraisalFee = appraisalFee;
                app.BankCharges = bankCharges;
            }
            catch (Exception)
            {
                app.AppraisalFee = 0;
                app.BankCharges = 0;
            }
            return app;
        }
    }
}
