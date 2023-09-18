using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContactManager.Models
{
    public class BusinessPerson
    {
        [Key]
        public int BusinessPersonID { get; set; }

        [ForeignKey("BusinessID")]
        public int BusinessID { get; set; }
        public Business? Business { get; set; }

        [ForeignKey("PersonID")]
        public int PersonID { get; set; }
        public Person? Person { get; set; }
    }
}
