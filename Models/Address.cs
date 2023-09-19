using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tmm.Models
{
    public class Address
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        [Required]
        [MaxLength(80)]
        public string AddressLine1 { get; set; }

        [MaxLength(80)]
        public string? AddressLine2 { get; set; }

        [Required]
        [MaxLength(50)]
        public string Town { get; set; }

        [MaxLength(50)]
        public string County { get; set; }

        [Required]
        [MaxLength(10)]
        public string Postcode { get; set; }

        [MaxLength(50)]
        public string Country { get; set; } = "UK"; // Default to UK if not provided

        public bool IsMain { get; set; } // To denote if it's the main address

        // Navigation property
        public Customer Customer { get; set; }
    }
}
