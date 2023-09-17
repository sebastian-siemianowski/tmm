using System.Collections.Generic;

namespace Tmm.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Forename { get; set; }
        public string Surname { get; set; }
        public string EmailAddress { get; set; }
        public string MobileNo { get; set; }
        public bool IsActive { get; set; } = true;
        public List<Address> Addresses { get; set; } = new List<Address>();
    }
}
