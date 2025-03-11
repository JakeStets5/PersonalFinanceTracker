using Newtonsoft.Json;

namespace PersonalFinanceTracker.Common.Models
{
    public class User
    {
        [JsonProperty("id")] // Maps to Cosmos DB's "id" field. Uses UserId as the property name
        public string Id
        {
            get => UserId;
            set => UserId = value;
        } 

        [JsonProperty("UserId")]
        public string UserId { get; set; }

        [JsonProperty("Username")]
        public string Username { get; set; }

        [JsonProperty("Email")]
        public string Email { get; set; }

        [JsonProperty("Password")]
        public string Password { get; set; }
    }
}
