
namespace ElegantEventsWeb.Models
{
    public class Event
    {
        public string Event_ID { get; set; } = "";
        public string Event_Name { get; set; } = "";
        public DateTime Event_Date { get; set; } = DateTime.Today;
        public int NumberOfGuests { get; set; }
        public decimal Budget { get; set; }
        public int Client_ID { get; set; }
        public string VenueName { get; set; } = "";
    }
}
