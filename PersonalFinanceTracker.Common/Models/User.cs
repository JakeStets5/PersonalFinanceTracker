using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace PersonalFinanceTracker.Common.Models
{
    public class User
    {
        [JsonProperty("userId")] // Maps to Cosmos DB's "id" field
        public string UserId { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
