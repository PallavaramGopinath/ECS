using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IPayPFRoiTemplateHandler
    {
        Task<bool> AddPayPFRoiTemplateAsync(Pay_PF_ROITemplate payPFROITemplate);
        Task<bool> EditPayPFRoiTemplateAsync(Pay_PF_ROITemplate payPFROITemplate);
        Task<List<Pay_PF_ROITemplate>> GetPayPFROITemplateListAsync();
    }
}
