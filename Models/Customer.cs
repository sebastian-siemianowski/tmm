using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tmm.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string Title { get; set; }

        [Required]
        [MaxLength(50)]
        public string Forename { get; set; }

        [Required]
        [MaxLength(50)]
        public string Surname { get; set; }

        [Required]
        [MaxLength(75)]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        [MaxLength(15)]
        [Phone]
        public string MobileNo { get; set; }

        public bool IsActive { get; set; } = true;  // Assuming a customer is active by default when added

        // Navigation property
        public List<Address> Addresses { get; set; }
    }
}
