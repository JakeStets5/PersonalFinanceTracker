using Newtonsoft.Json;

namespace PersonalFinanceTracker.Common.Models
{
    public class Statement
    {
        [JsonProperty("id")] // Cosmos expects "id" in JSON. Mapping to statementId
        public string Id
        {
            get => StatementId;
            set => StatementId = value;
        }

        [JsonProperty("UserId")]
        public string UserId { get; set; }

        [JsonProperty("StatementId")]
        public string StatementId { get; set; }

        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty("Amount")]
        public decimal Amount { get; set; }

        [JsonProperty("Source")]
        public string Source { get; set; }

        [JsonProperty("Frequency")]
        public string Frequency { get; set; }

        [JsonProperty("Date")]
        public DateTime Date { get; set; }

        [JsonProperty("PaymentMethod")]
        public string PaymentMethod { get; set; }
    }
}
