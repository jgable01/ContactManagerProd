using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContactManager.Models
{
    public class AddressPerson
    {
        [Key]
        public int AddressPersonID { get; set; }

        [ForeignKey("AddressID")]
        public int AddressID { get; set; }
        public Address? Address { get; set; }

        [ForeignKey("PersonID")]
        public int PersonID { get; set; }
        public Person? Person { get; set; }
    }
}
