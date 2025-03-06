using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using System.Text.Json.Serialization;

namespace PersonalFinanceTracker.Common.Models
{
    public class Statement
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonPropertyName("id")]
        public string StatementId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("Source")]
        public string Source { get; set; }

        [JsonPropertyName("frequency")]
        public string Frequency { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("paymentMethod")]
        public string PaymentMethod { get; set; }
    }
}
