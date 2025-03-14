using Microsoft.Azure.Cosmos;
using PersonalFinanceTracker.Common.Interfaces;
using PersonalFinanceTracker.AzureApi.Services;
using Microsoft.Extensions.Logging;
using PersonalFinanceTracker.AzureApi.Controllers;
using Microsoft.AspNetCore.Mvc;

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
    if (string.IsNullOrEmpty(config["Endpoint"]) || string.IsNullOrEmpty(config["Key"]))
    {
        throw new ArgumentException("CosmosDb config is incomplete");
    }
    var client = new CosmosClient(config["Endpoint"], config["Key"],
        new CosmosClientOptions { ApplicationName = "PersonalFinanceTracker.AzureApi" });
    return client;
});

builder.Services.AddScoped<ICloudDbService, CosmosDbService>();

//builder.Services.AddScoped<UserController>();
builder.Services.AddControllers()
    .AddControllersAsServices();

// Swagger setup
builder.Services.AddEndpointsApiExplorer(); // Required for Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PersonalFinanceTracker API", Version = "v1" });
});

// Log registered controllers
var controllerTypes = typeof(Program).Assembly.GetTypes()
    .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && t.Name.EndsWith("Controller"));

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

app.Run();