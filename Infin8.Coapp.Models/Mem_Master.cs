using System.ComponentModel.DataAnnotations;
namespace Infin8.Coapp.Models
{
    using System;
    public partial class mem_master
    {
        [Key]
        public decimal mem_id { get; set; }
        public  string? memberno { get; set; }
        public int membertype { get; set; }
        public string? perno { get; set; }
        public string? resolutionno { get; set; }
        public DateTime? resolutiondate { get; set; }
        public string? memberphoto { get; set; }
        public string? memberesignature { get; set; }
        public string? salutation { get; set; }
        public string? membername { get; set; }
        public string? relation_type { get; set; }
        public string? fathername { get; set; }
        public int gender { get; set; }
        public decimal caste_id { get; set; }
        public DateTime? dob { get; set; }
        public int age { get; set; }
        public string? alternative_mobileno { get; set; }
        public string? officephoneno { get; set; }
        public string? mobileno { get; set; }
        public string? emailid { get; set; }
        public DateTime? doj { get; set; }
        public DateTime? dor { get; set; }
        public decimal designation_id { get; set; }
        public bool ispermanent { get; set; }
        public decimal office_id { get; set; }
        public string? tickettokengangno { get; set; }
        public string? passingofficername { get; set; }
        public string? preadd1 { get; set; }
        public string? preadd2 { get; set; }
        public string? preadd3 { get; set; }
        public string? prepin { get; set; }
        public int precity { get; set; }
        public decimal prearea_id { get; set; }
        public string? peradd1 { get; set; }
        public string? peradd2 { get; set; }
        public string? peradd3 { get; set; }
        public string? perpin { get; set; }
        public int percity { get; set; }
        public decimal perarea_id { get; set; }
        public double basicpay { get; set; }
        public int memberstatus { get; set; }
        public bool isaccountclosed { get; set; }
        public DateTime? accountcloseddate { get; set; }
        public bool isexistingmember { get; set; }
        public string? existing_memberno { get; set; }
        public DateTime? existingdoc { get; set; }
        public bool ismember_othersociety { get; set; }
        public string? nomineename { get; set; }
        public int nomineeage { get; set; }
        public string? nomineerelationship { get; set; }
        public bool memberdelete { get; set; }
        public bool isnewmember { get; set; }
        public decimal suretymem_id { get; set; }
        public decimal usr_id { get; set; }
        public decimal yr_id { get; set; }
        public double income { get; set; }
        public decimal comm_id { get; set; }
        public decimal occ_id { get; set; }
        public decimal religion_id { get; set; }
        public bool member_oe { get; set; }
        public string? sectioncode { get; set; }
        public string? inactivestatus { get; set; }
        public bool ismemexpired { get; set; }
        public DateTime? expireddate { get; set; }
        public string? designation { get; set; }
        public string? section_name { get; set; }
        public int factorysection_id { get; set; }
        public int factorytrade_id { get; set; }
        public string? sbaccountno { get; set; }
        public string? bankname { get; set; }
        public string? ifsccode { get; set; }
        public DateTime? admissiondate { get; set; }
        public string? token_personno { get; set; }
        public int grosspay { get; set; }
        public string? gpf_no { get; set; }
        public string? panno { get; set; }
        public string? aadharno { get; set; }
        public string? smartcardno { get; set; }
        public string? aadharcardpath { get; set; }
        public string? brcode { get; set; }
        public int constituency_id { get; set; }
    }
}
