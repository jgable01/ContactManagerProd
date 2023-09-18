using System.ComponentModel.DataAnnotations;

namespace ContactManager.Models
{
    public class Business
    {
        [Key]
        public int BusinessID { get; set; }

        [Display(Name = "Name of Business")]
        [Required]
        [MaxLength(100)]
        public string BusinessName { get; set; }

        [Display(Name = "Phone Number")]
        [Required]
        [Phone]
        public string? PhoneNumber { get; set; }

        [RegularExpression(@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage ="Invalid Email")] // Email format validation 
        [Required]
        public string Email { get; set; }

        // New relationship with Address

        public HashSet<BusinessPerson> BusinessPeople { get; set; } = new HashSet<BusinessPerson>();

        public HashSet<Address> Addresses { get; set; } = new HashSet<Address>();
    }
}
