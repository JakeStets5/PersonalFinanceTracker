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

        public async Task<User?> SignInAsync(string username, string password)
        {
            try
            {
                // Create anonymous object for POST body—matches SignInRequest in API
                var request = new { Username = username, Password = password };

                // Send POST to API sign-in endpoint—expects 200 OK with User JSON or 401 Unauthorized
                var response = await _httpClient.PostAsJsonAsync("api/user/signin", request);

                // Check if response is successful (200-299)—non-success returns null for invalid creds
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                // Read raw JSON response
                var content = await response.Content.ReadAsStringAsync();

                // Deserialize JSON into User object. Null if JSON mismatches User model
                var user = JsonSerializer.Deserialize<User>(content);
                return user;
            }
            catch (ObjectDisposedException ex)
            {
                Debug.WriteLine($"Disposed error: {ex}");
                return null;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"HTTP error: {ex.Message}, Status: {ex.StatusCode}");
                var errorContent = ex.InnerException?.Message ?? "No content";
                Debug.WriteLine($"Error content: {errorContent}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error: {ex}");
                return null;
            }
        }

        public async Task<Statement?> SubmitStatementAsync(Statement statement)
        {
            var response = await _httpClient.PostAsJsonAsync("api/statements", statement);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Statement>();
        }

        public async Task<List<Statement?>> GetStatementsAsync(string userId)
        {
            return await _httpClient.GetFromJsonAsync<List<Statement>>($"api/statements/{userId}");
        }
    }
}
