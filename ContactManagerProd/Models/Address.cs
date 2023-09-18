using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

namespace ContactManager.Models
{
    public class Address
    {
        [Key]
        public int AddressID { get; set; }

        [Display(Name = "Street Name")]
        [Required]
        [MaxLength(100)]
        public string StreetName { get; set; }

        [Display(Name = "Street Number")]
        [Required]
        public int StreetNumber { get; set; }

        [Display(Name = "Unit # (Not Required)")]
        public int? UnitNumber { get; set; } // Made this nullable as not all addresses might have a unit number

        [Display(Name = "Postal Code")]
        [Required]
        [RegularExpression(@"^[ABCEGHJ-NPRSTVXY]\d[ABCEGHJ-NPRSTV-Z][ -]?\d[ABCEGHJ-NPRSTV-Z]\d$", ErrorMessage ="Invalid postal code")] // Fixed regex pattern
        public string PostalCode { get; set; }

        public HashSet<AddressPerson> AddressPeople { get; set; } = new HashSet<AddressPerson>();

        // New relationship with Business
        [ForeignKey("BusinessID")]
        public int? BusinessID { get; set; }
        public Business? Business { get; set; } // Navigation property
    }
}
