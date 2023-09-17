namespace Tmm.Models
{
    public class Address
    {
        public int Id { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; } = "UK";
        public bool IsMain { get; set; } = false;

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
    }
}
