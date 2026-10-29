
namespace ElegantEventsWeb.Models
{
    public class Vendor
    {
        public string VEmail { get; set; } = "";
        public string VAddress { get; set; } = "";
        public string VFName { get; set; } = "";
        public string VLName { get; set; } = "";
        public string ContactPersonnel { get; set; } = "";
        public string VPhone { get; set; } = "";
        public string? SubvendorEmail { get; set; }
    }
}
