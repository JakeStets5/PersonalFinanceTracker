using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using PersonalFinanceTracker.Common.Models;
using PersonalFinanceTracker.Backend.Interfaces;
using System.Diagnostics;
using Newtonsoft.Json;

namespace PersonalFinanceTracker.Backend.Services
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;

        public ApiClient(HttpClient httpClient) 
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5251/") // Local API
            };
        }

        /// <summary>
        /// Targets the API's user endpoint to retrieve a user by username.
        /// </summary>
        /// <param name="username">The username of the user to retrieve</param>
        /// <exception cref="Exception">Thrown if the upsert fails, with details logged.</exception>
        /// <returns> A <see cref="User"/> if found, or null if not found or an error occurred.</returns>
        /// <remarks>
        /// sends the username to a GET endpoint on the API, expecting a User object in return.
        /// </remarks>
        public async Task<User?> GetUserAsync(string username)
        {
            try
            {
                // Send GET to API user endpoint
                return await _httpClient.GetFromJsonAsync<User>($"api/user/{username}");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error getting user: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Targets the API's user endpoint to sign in a user.
        /// </summary>
        /// <param name="username">The username of the user to retrieve</param>
        /// /// <param name="password">The password of the user to retrieve</param>
        /// <exception cref="Exception">Thrown if the upsert fails, with details logged.</exception>
        /// <returns> A <see cref="User"/> if found, or null if not found or an error occurred.</returns>
        /// <remarks>
        /// sends the username + password to a GET endpoint on the API, expecting a User object in return.
        /// </remarks>
        public async Task<(User? User, string? Error)> SignInAsync(string username, string password)
        {
            try
            {
                // Create anonymous object for POST body
                var request = new { Username = username, Password = password };

                // Send POST to API sign-in endpoint
                var response = await _httpClient.PostAsJsonAsync("api/user/signin", request);

                // Check if response is successful. Non-success returns null for invalid creds
                if (!response.IsSuccessStatusCode)
                {
                    var status = response.StatusCode;
                    var error = status == System.Net.HttpStatusCode.NotFound
                        ? "Username not found"
                        : "Username or password incorrect";
                    return (null, error);
                }

                // Read raw JSON response
                var content = await response.Content.ReadAsStringAsync();

                // Deserialize JSON into User object. Null if JSON mismatches User model
                var user = JsonConvert.DeserializeObject<User>(content);
                return (user, null);
            }
            catch (ObjectDisposedException ex)
            {
                Debug.WriteLine($"Disposed error: {ex}");
                return (null, ex.ToString());
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"HTTP error: {ex.Message}, Status: {ex.StatusCode}");
                var errorContent = ex.InnerException?.Message ?? "No content";
                return (null, ex.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error: {ex}");
                return (null, ex.ToString());
            }
        }
         
         /// <summary>
        /// Targets the API's statement endpoint to upsert a statement.
        /// </summary>
        /// <param name="statement">The statement to upsert</param>
        /// <exception cref="Exception">Thrown if the upsert fails, with details logged.</exception>
        /// <returns> A <see cref="Statement"/> if found, or null if not found or an error occurred.</returns>
        /// <remarks>
        /// sends the statement to a POST endpoint on the API, expecting a Statement object in return.
        /// </remarks>
        public async Task<Statement?> SubmitStatementAsync(Statement statement)
        {
            // Serialize statement to JSON
            var json = JsonConvert.SerializeObject(statement);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Send POST to API statement endpoint
            var response = await _httpClient.PostAsync("api/statement", content);

            // Fetch statements for userId from API. Expects 200 OK with list
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("POST failed: {Error}", error);
                return null;
            }

            // Read raw JSON response
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Statement>(responseContent);
        }

        public async Task<User?> CreateUserAsync(User user)
        {
            // Serialize user to JSON
            var json = JsonConvert.SerializeObject(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Send POST to API user endpoint
            var response = await _httpClient.PostAsync("api/user", content);
            Debug.WriteLine("POST response: {response}", response);

            // Fetch statements for userId from API. Expects 200 OK with list
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("POST failed: {Error}", error);
                return null;
            }

            // Read raw JSON response
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<User>(responseContent);
        }

        /// <summary>
        /// Targets the API's statement endpoint to fetch statements.
        /// </summary>
        /// <param name="userId">Partition key to fetch statements by</param>
        /// <param name="startDate">Date filtering</param>
        /// <param name="endDate">Date filtering</param>
        /// <exception cref="Exception">Thrown if the upsert fails, with details logged.</exception>
        /// <returns> A list of <see cref="Statement"/> if found, or null if not found or an error occurred.</returns>
        /// <remarks>
        /// sends the statement to a POST endpoint on the API, expecting a Statement object in return.
        /// </remarks>
        public async Task<IEnumerable<Statement?>> GetStatementsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = $"api/statement?userId={userId}";

            // Append date filters to query string
            if (startDate.HasValue)
                query += $"&startDate={startDate.Value:yyyy-MM-dd}";
            if (endDate.HasValue)
                query += $"&endDate={endDate.Value:yyyy-MM-dd}";

            // Send POST to API statement endpoint
            var response = await _httpClient.GetAsync(query);

            // Fetch statements for userId from API. Expects 200 OK with list
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            // Read raw JSON response
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<Statement>>(content);
        }
    }
}