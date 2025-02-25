using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;

namespace PersonalFinanceTracker.Backend.Models
{
    [DynamoDBTable("Statement")]
    public class Statement
    {
        [DynamoDBHashKey]
        public string UserId { get; set; }  // Partition key

        [DynamoDBRangeKey]
        public string StatementId { get; set; }  // Sort key, could be a GUID

        public string Type { get; set; } // "Income" or "Expense"
        public decimal Amount { get; set; }
        public string Source { get; set; }
        public string Frequency { get; set; }
        public DateTime Date { get; set; }
        public string PaymentMethod { get; set; }
    }
}
