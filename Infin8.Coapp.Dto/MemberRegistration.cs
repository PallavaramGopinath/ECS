using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class MemberRegistration
    {
        /// <summary>
        /// basic information
        /// </summary>
        //[Required(ErrorMessage = "Member number is required.")]
        //[StringLength(15)]
        public string? memberno { get; set; }

        [Required(ErrorMessage = "Member type is required.")]
        [Range(1,3)]
        public int membertype { get; set; }
        public string? resolutionno { get; set; }
        public DateTime? resolutiondate { get; set; }
        [Required(ErrorMessage = "Admission date is required")]
        [Column(TypeName = "Date")]
        public DateTime? admissiondate { get; set; }

        [Required(ErrorMessage = "Salutation is required")]
        [StringLength(10)]
        public string? salutation { get; set; }
        [Required(ErrorMessage = "Member name is required.")]
        [StringLength(125)]
        public string? membername { get; set; }

        [Required(ErrorMessage = "Relation type is required.")]
        [StringLength(10)]
        public string? relation_type { get; set; }

        [Required(ErrorMessage = "Father name is required.")]
        [StringLength(125)]
        public string? fathername { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        [Range(1, 3, ErrorMessage = "Please select a valid gender")]
        public int gender { get; set; }
        [Required(ErrorMessage = "Religion is required.")]
        [Range(1, 999999999999, ErrorMessage = "Please select a valid religion")]
        public decimal religion_id { get; set; } /// religion  dropdown list from database table
        [Required(ErrorMessage = "Community is required.")]
        [Range(1, 999999999999, ErrorMessage = "Please select a valid community")]
        public decimal comm_id { get; set; } /// community dropdown list from database table
        public decimal caste_id { get; set; } /// caste dropdown list from database table
        public decimal occ_id { get; set; } /// occupation dropdown list from database table
        [Required(ErrorMessage = "Date of birth is required")]
        //[DateOfBirthValidation(ErrorMessage = "Relative Date of birth is required")]
        [Column(TypeName = "Date")]
        public DateTime? dob { get; set; }

        [Required(ErrorMessage = "Age is required.")]
        [Range(1, 100, ErrorMessage = "Please enter a valid age between 1 and 100")]
        public int age { get; set; }
        /// <summary>
        /// contact information
        /// </summary>
        public string? alternative_mobileno { get; set; }
        public string? officephoneno { get; set; }
        [Required(ErrorMessage = "Mobile number is required.")]
        [StringLength(50)]
        public string? mobileno { get; set; }
        public string? emailid { get; set; }

        /// <summary>
        /// present address
        /// </summary>
        [Required(ErrorMessage = "Present address line1 is required.")]
        [StringLength(100)]
        public string? preadd1 { get; set; }
        
        public string? preadd2 { get; set; }
        public string? preadd3 { get; set; }
        [Required(ErrorMessage = "Present address pin is required.")]
        [StringLength(100)]
        public string? prepin { get; set; }
        public decimal prearea_id { get; set; }  /// area name dropdown list from database table

        /// <summary>
        /// permanent address
        /// </summary>
        [Required(ErrorMessage = "Permanent address is required.")]
        [StringLength(100)]
        public string? peradd1 { get; set; }
        public string? peradd2 { get; set; }
        public string? peradd3 { get; set; }
        [Required(ErrorMessage = "Permanent address pin is required.")]
        [StringLength(100)]
        public string? perpin { get; set; }
        public decimal perarea_id { get; set; } /// area name dropdown list from database table

        /// <summary>
        /// membership deails
        /// <summary>
        public bool isnewmember { get; set; }
        public bool isexistingmember { get; set; }
        public string? existing_memberno { get; set; }
        public bool ismember_othersociety { get; set; }

        /// <summary>
        /// nominee information
        /// </summary>
        [Required(ErrorMessage = "Nominee name is required.")]
        [StringLength(50)]
        public string? nomineename { get; set; }


        [Required(ErrorMessage = "Nominiee age is required.")]
        [Range(1, 100, ErrorMessage = "Please enter a valid nominee age between 1 and 100")]
        public int nomineeage { get; set; }
        [Required(ErrorMessage = "Nominee relationship is required.")]
        [StringLength(50)]
        public string? nomineerelationship { get; set; }

        /// <summary>
        /// bank and identity information
        /// </summary>
        public string? sbaccountno { get; set; }
        public string? bankname { get; set; }
        public string? ifsccode { get; set; }

        public string? gpf_no { get; set; }

        [Required(ErrorMessage = "PAN number is required.")]
        [StringLength(50)]
        public string? panno { get; set; }

        [Required(ErrorMessage = "Aadhar number is required.")]
        [StringLength(50)]
        public string? aadharno { get; set; }

        [Required(ErrorMessage = "Smart card number is required.")]
        [StringLength(50)]
        public string? smartcardno { get; set; }
        public string? aadharcardpath { get; set; }

        /// <summary>
        /// photo and documentation
        /// </summary>
        public string? memberphoto { get; set; }
        public string? memberesignature { get; set; }
        public int constituency_id { get; set; }

        public string? brcode { get; set; }

    }
}
