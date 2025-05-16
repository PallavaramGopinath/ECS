using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IPayDATemplateRepository
    {
        Task<bool> AddPayDATemplateAsync(Pay_DA_Template payDaTemplate);
        Task<bool> EditPayDATemplateAsync(Pay_DA_Template payDaTemplate);
        Task<List<Pay_DA_Template>> GetPayDATemplateListAsync(string status);
    }
}
