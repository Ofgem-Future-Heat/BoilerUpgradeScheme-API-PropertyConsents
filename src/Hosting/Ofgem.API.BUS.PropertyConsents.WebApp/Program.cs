using Azure.Identity;
using Ofgem.API.BUS.Applications.Client;
using Ofgem.API.BUS.PropertyConsents.API.Controllers;
using Ofgem.API.BUS.PropertyConsents.Core;
using Ofgem.Lib.BUS.Logging;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddOfgemCloudApplicationInsightsTelemetry();

// Azure Key Vault configuration
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
    new DefaultAzureCredential());

// make something somewhere else for this to kinda work better?
builder.Services.AddMvc().AddApplicationPart(typeof(OwnerConsentController).GetTypeInfo().Assembly);
builder.Services.AddApplicationsAPIClient(builder.Configuration, "ApplicationsAPIBaseAddress");
builder.Services.AddServiceConfigurations(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseTelemetryMiddleware();
app.Run();
