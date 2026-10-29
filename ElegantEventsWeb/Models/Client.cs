
namespace ElegantEventsWeb.Models
{
    public class Client
    {
        public int Client_ID { get; set; }
        public string Cfname { get; set; } = "";
        public string Clname { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string BillingAddress { get; set; } = "";
    }
}
