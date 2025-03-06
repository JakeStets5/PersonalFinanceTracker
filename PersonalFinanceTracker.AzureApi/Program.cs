using Microsoft.Azure.Cosmos;
using PersonalFinanceTracker.Common.Interfaces;
using PersonalFinanceTracker.AzureApi.Services;
using Microsoft.Extensions.Logging;
using PersonalFinanceTracker.AzureApi.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

builder.Services.AddSingleton<CosmosClient>(sp =>
{
    var config = builder.Configuration.GetSection("CosmosDb");
    var logger = sp.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Attempting to create CosmosClient - Endpoint: {Endpoint}, Key: {Key}",
        config["Endpoint"], config["Key"]?[..5] + "...");
    if (string.IsNullOrEmpty(config["Endpoint"]) || string.IsNullOrEmpty(config["Key"]))
    {
        logger.LogError("CosmosDb config missing");
        throw new ArgumentException("CosmosDb config is incomplete");
    }
    var client = new CosmosClient(config["Endpoint"], config["Key"],
        new CosmosClientOptions { ApplicationName = "PersonalFinanceTracker.AzureApi" });
    logger.LogInformation("CosmosClient created successfully");
    return client;
});

builder.Services.AddScoped<ICloudDbService, CosmosDbService>();
builder.Services.AddScoped<UserController>();
builder.Services.AddControllers()
    .AddControllersAsServices();

// Swagger setup
builder.Services.AddEndpointsApiExplorer(); // Required for Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PersonalFinanceTracker API", Version = "v1" });
});

var app = builder.Build();
app.Logger.LogInformation("API starting up");

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PersonalFinanceTracker API v1"));
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

//app.MapGet("/test", async context =>
//{
//    await context.Response.WriteAsync("Test endpoint works!");
//});

//app.MapGet("/api/user/{username}", async context =>
//{
//    await context.Response.WriteAsync($"Fallback hit for username: {context.Request.RouteValues["username"]}");
//});
//app.MapControllers(); // After fallback

// Log registered controllers
var controllerTypes = builder.Services
    .Where(s => s.ServiceType == typeof(UserController))
    .ToList();
app.Logger.LogInformation("Registered {Count} controller types", controllerTypes.Count);

app.Run();