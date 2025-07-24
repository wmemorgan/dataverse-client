using ConsoleDemo;
using Dataverse.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Create host with dependency injection
HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// Add user secrets configuration
builder.Configuration.AddUserSecrets<Program>();

// Add Dataverse client services
builder.Services.AddDataverseClient(builder.Configuration, "Dataverse");

// Add logging
builder.Services.AddLogging(logging =>
    logging.AddConsole().SetMinimumLevel(LogLevel.Information));

// Register application services
builder.Services.AddScoped<IDataverseOperations, DataverseOperations>();
builder.Services.AddScoped<IUserInterface, ConsoleUserInterface>();
builder.Services.AddScoped<DemoApplication>();

IHost host = builder.Build();

// Debug: Print configuration values to verify they're being loaded
IConfiguration config = host.Services.GetRequiredService<IConfiguration>();
Console.WriteLine($"Dataverse:Url = {config["Dataverse:Url"]}");
Console.WriteLine($"Dataverse:ClientId = {config["Dataverse:ClientId"]}");
Console.WriteLine(
    $"Dataverse:ClientSecret = {(string.IsNullOrEmpty(config["Dataverse:ClientSecret"]) ? "NOT FOUND" : "FOUND")}");

// Run the demo application
DemoApplication app = host.Services.GetRequiredService<DemoApplication>();
await app.RunAsync();
