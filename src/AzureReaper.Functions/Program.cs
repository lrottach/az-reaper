using Azure.Identity;
using Azure.ResourceManager;
using AzureReaper.Interfaces;
using AzureReaper.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddSingleton<DefaultAzureCredential>();
builder.Services.AddSingleton(sp => new ArmClient(sp.GetRequiredService<DefaultAzureCredential>()));
builder.Services.AddSingleton<IAzureResourceService, AzureResourceService>();

builder.Build().Run();
