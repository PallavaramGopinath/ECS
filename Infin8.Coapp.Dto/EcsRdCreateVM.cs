using System;
using System.Collections.Generic;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.Dto
{
    /// <summary>
    /// ECS RD New Account, acc 17 (spec 15 §9). Serialised into staging_details.related_account_data at Initiate and
    /// recomputed at approval. The RD number is allocated at approval; ProvisionalTD_No is a display hint only.
    /// </summary>
    public class DtoEcsRdCreate
    {
        public decimal MemId { get; set; }                 // transacting member = first holder
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public DateTime ApplicationDate { get; set; }      // business day = AccountOpenDate
        public int CashOrAdjustment { get; set; }          // 1 cash, >1 adjustment (as voc_Trn_Type)
        public long TDScheme_Id { get; set; }
        public string? TDScheme_Name { get; set; }
        public string? ProvisionalTD_No { get; set; }      // display only; allocated on approval
        public DateTime ValueDate { get; set; }
        public double Instalment { get; set; }             // DepositAmount = staging line amount
        public int PeriodInMonths { get; set; }
        public double RateOfInterest { get; set; }
        public double PenalRate { get; set; }
        public DateTime MaturityDate { get; set; }
        public double MaturityAmount { get; set; }
        public bool IncludeInDemand { get; set; } = true;  // Status 'D' / 'N'
        public int ModeOfOperation { get; set; } = 1;
        public string? NameOnBond { get; set; }
        public List<DtoEcsRdCreateHolder> Holders { get; set; } = new();
        public string? Nominee1Name { get; set; }
        public int Nominee1Age { get; set; }
        public string? Nominee1Relationship { get; set; }
        public string? Nominee2Name { get; set; }
        public int Nominee2Age { get; set; }
        public string? Nominee2Relationship { get; set; }
    }

    public class DtoEcsRdCreateHolder
    {
        public decimal Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public string? FatherName { get; set; }
        public int Age { get; set; }                       // as on the business day
        public int SlNo { get; set; }                      // 1 = first holder
    }

    /// <summary>An RD scheme offered by the component (spec 15 §4.1).</summary>
    public class DtoEcsRdScheme
    {
        public long TDScheme_Id { get; set; }
        public string? TDScheme_Name { get; set; }
        public int MinimumPeriod { get; set; }
        public int MaximumPeriod { get; set; }
    }

    /// <summary>Everything the Maker screen needs on load (spec 15 §4).</summary>
    public class EcsRdCreateSetupVM
    {
        public DateTime ApplicationDate { get; set; }
        public string? Message { get; set; }               // set when the screen cannot be used
        public List<DtoEcsRdScheme> Schemes { get; set; } = new();
        public List<TermDeposit_Roi_Template> RoiRows { get; set; } = new();
        public string? ProvisionalTD_No { get; set; }
        public DtoEcsRdCreateHolder? FirstHolder { get; set; }
        public DtoNominee? LastNominee { get; set; }
    }

    /// <summary>A joint holder lookup by member number (spec 15 §4.2).</summary>
    public class EcsRdHolderLookupVM
    {
        public DtoEcsRdCreateHolder? Holder { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>The server's recomputation of a staged opening (spec 15 §6, §7).</summary>
    public class EcsRdCreateCalcVM
    {
        public DtoEcsRdCreate Dto { get; set; } = new();
        public string? Message { get; set; }               // null = valid
    }

    /// <summary>Member fields the server needs to check a holder (spec 15 §7.3).</summary>
    public class DtoEcsRdMember
    {
        public decimal Mem_Id { get; set; }
        public string? MemberNo { get; set; }
        public string? MemberName { get; set; }
        public string? FatherName { get; set; }
        public DateTime? Dob { get; set; }
        public int MemberStatus { get; set; }
        public bool IsExpired { get; set; }
        public bool IsDeleted { get; set; }
    }
}
