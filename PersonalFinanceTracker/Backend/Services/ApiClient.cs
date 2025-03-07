using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using PersonalFinanceTracker.Common.Models;
using PersonalFinanceTracker.Backend.Interfaces;
using System.Text.Json;
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
                BaseAddress = new Uri("http://localhost:5251/")
            };
            Console.WriteLine("ApiClient created");
        }

        public async Task<User?> GetUserAsync(string username)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<User>($"api/user/{username}");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error getting user: {e.Message}");
                return null;
            }
        }

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
                Debug.WriteLine($"Error content: {errorContent}");
                return (null, ex.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error: {ex}");
                return (null, ex.ToString());
            }
        }

        public async Task<Statement?> SubmitStatementAsync(Statement statement)
        {
            var response = await _httpClient.PostAsJsonAsync("api/statement", statement);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Statement>(content);
        }

        public async Task<IEnumerable<Statement?>> GetStatementsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = $"api/statement?userId={userId}";
            if (startDate.HasValue)
                query += $"&startDate={startDate.Value:yyyy-MM-dd}";
            if (endDate.HasValue)
                query += $"&endDate={endDate.Value:yyyy-MM-dd}";

            // Fetch statements for userId from API. Expects 200 OK with list
            var response = await _httpClient.GetAsync(query);
            if(!response.IsSuccessStatusCode)
            {
                return null;
            }
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<Statement>>(content);
        }
    }
}
