
using System.ComponentModel.DataAnnotations;

namespace ElegantEventsWeb.Models
{
    public class Payment
    {
        public int PaymentNumber { get; set; }

        [Required]
        public string VEmail { get; set; } = "";

        [Required]
        public string Event_ID { get; set; } = "";

        public DateTime ContractDate { get; set; }

        [Range(typeof(decimal), "0.01", "999999999999999999.99")]
        public decimal AmountPaid { get; set; }

        public decimal BalanceOutstanding { get; set; }

        [Required]
        public string MethodPay { get; set; } = "Cash";
    }
}
