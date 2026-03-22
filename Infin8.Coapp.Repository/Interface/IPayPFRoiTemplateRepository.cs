using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IPayPFRoiTemplateRepository
    {
        Task<List<Pay_PF_ROITemplate>> AddPayPFRoiTemplateAsync(Pay_PF_ROITemplate payPFROITemplate);
        Task<List<Pay_PF_ROITemplate>> EditPayPFRoiTemplateAsync(Pay_PF_ROITemplate payPFROITemplate);
        Task<List<Pay_PF_ROITemplate>> GetPayPFROITemplateListAsync(string brCode);
        Task<Pay_PF_ROITemplate> GetPayPFRoiTemplateByDate(DateTime salaryDate,string brCode);
    }
}
