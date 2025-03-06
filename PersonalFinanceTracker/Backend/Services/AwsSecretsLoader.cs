using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Backend.Services;

public class AwsSecretsLoader
{
    public static async Task LoadDynamoDBCredentials()
    {
        string secretName = "DynamoDBCredentials";
        string region = "us-east-2";

        IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

        GetSecretValueRequest request = new GetSecretValueRequest
        {
            SecretId = secretName,
            VersionStage = "AWSCURRENT"
        };

        GetSecretValueResponse response;

        try
        {
            response = await client.GetSecretValueAsync(request);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving secret: {ex.Message}");
            throw;
        }

        if (response.SecretString != null)
        {
            // Deserialize the secret into a configuration object
            var credentials = JsonConvert.DeserializeObject<DynamoDBCredentials>(response.SecretString);

            // Inject into environment variables or configuration
            Environment.SetEnvironmentVariable("AccessKey", credentials?.AccessKey);
            Environment.SetEnvironmentVariable("SecretKey", credentials?.SecretKey);

            Console.WriteLine("DynamoDB credentials loaded successfully.");
        }
        else
        {
            Console.WriteLine("Secret retrieval failed: No secret string found.");
        }
    }
}

// Credentials class to match the JSON structure from Secrets Manager
public class DynamoDBCredentials
{
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
}
