using Infin8.Coapp.Models;
namespace Infin8.Coapp.Dto
{
    public class MemGIForViewVM
    {
        public Mem_GI_Master?  memGIMaster { get; set; }
        public Mem_GI? memGI { get; set; }
        //public List<Mem_GI_Trn> memGITrnList { get; set; }

        public List<MemberGroupInsuranceVM>? MemGroupInsuranceList { get; set; }
    }
}
