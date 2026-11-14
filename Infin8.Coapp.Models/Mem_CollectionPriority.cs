using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Infin8.Coapp.Models
{
    /// <summary>
    /// Order in which a member's salary recovery is appropriated across a demand (spec 13 §4): DPI / D by
    /// <see cref="Deposit_Id"/>, LPI / LI / LP / LP1–LP3 by <see cref="Scheme_Id"/>, DTO / DBY by
    /// <see cref="PriorityLed_Id"/>, RD and LPA. The table is <c>mem_collection_priority</c> (with the underscore);
    /// its id columns are nullable and <c>priorityled_id</c> is a 12-digit id.
    /// </summary>
    [Table("mem_collection_priority")]
    public partial class Mem_CollectionPriority
    {
        [Key]
        public decimal Priority_Id { get; set; }
        public int? PriorityNo { get; set; }
        public string? PriorityType { get; set; }
        public int? Scheme_Id { get; set; }
        public int? Deposit_Id { get; set; }
        public string? PriorityName { get; set; }
        public decimal? PriorityLed_Id { get; set; }
        public bool Is_Active { get; set; }
        public string? BrCode { get; set; }
        public decimal Usr_Id { get; set; }
    }
}
