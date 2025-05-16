using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IPayPFTemplateRepository
    {
        Task<bool> AddPayPFTemplateAsync(Pay_PF_Template payPFTemplate);
        Task<bool> EditPayPFTemplateAsync(Pay_PF_Template payPFTemplate);
        Task<List<Pay_PF_Template>> GetPayPFTemplateListAsync();
    }
}
