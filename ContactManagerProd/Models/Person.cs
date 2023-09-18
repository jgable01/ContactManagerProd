using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using ContactManager.Models;

namespace ContactManager.Models
{
    public class Person
    {
        [Key]
        public int PersonID { get; set; }

        [Display(Name = "First Name")]
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }


        [RegularExpression(@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage ="Invalid Email")] // Email format validation 
        [Required]
        public string Email { get; set; }

        [Display(Name = "Phone Number")]
        [Required]
        [Phone]
        public string? PhoneNumber { get; set; }


        public HashSet<BusinessPerson> BusinessPeople { get; set; } = new HashSet<BusinessPerson>();
        public HashSet<AddressPerson> AddressPeople { get; set; } = new HashSet<AddressPerson>();

    }
}
