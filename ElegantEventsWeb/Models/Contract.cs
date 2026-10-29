
namespace ElegantEventsWeb.Models
{
    public class Contract
    {
        public string VEmail { get; set; } = "";
        public string Event_ID { get; set; } = "";
        public DateTime ContractDate { get; set; }
        public string PaymentStatus { get; set; } = "";
        public decimal Cost { get; set; }
        public decimal OutstandingBalance { get; set; }
        public string EventName { get; set; } = "";
    
    public string VendorName { get; set; } = "";
        public string SelectedEventName { get; set; } = "";
    }
}
