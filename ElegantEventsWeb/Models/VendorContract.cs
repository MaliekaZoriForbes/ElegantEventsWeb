
namespace ElegantEventsWeb.Models
{
    public class VendorContract
    {
        public string VEmail { get; set; } = "";
        public string Event_ID { get; set; } = "";
        public string Event_Name { get; set; } = "";
        public DateTime Event_Date { get; set; }
        public DateTime ContractDate { get; set; }
        public string PaymentStatus { get; set; } = "";
        public decimal Cost { get; set; }
    }
}
