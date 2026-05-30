using DiagnosticsLab.Api.Configuration;
using DiagnosticsLab.Api.Data;
using DiagnosticsLab.Api.Endpoints;
using DiagnosticsLab.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services
    .AddOptions<ExternalServicesOptions>()
    .Bind(builder.Configuration.GetSection(ExternalServicesOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(options => Uri.TryCreate(options.BillingApiBaseUrl, UriKind.Absolute, out _), "Billing API base URL must be an absolute URI.")
    .ValidateOnStart();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=diagnostics-lab.db";
    options.UseSqlite(connectionString);
});

builder.Services.AddScoped<FakeShippingClient>();
builder.Services.AddScoped<FakeInventoryClient>();
builder.Services.AddSingleton<FakeStartupDependency>();
builder.Services.AddSingleton<FakeAuditSink>();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("Application is running."));

var app = builder.Build();

await DataSeeder.SeedAsync(app.Services);

app.MapGet("/", () => Results.Ok(new
{
    Name = ".NET Production Diagnostics Lab",
    Description = "Small ASP.NET Core lab for production-style diagnostics scenarios."
}));

app.MapOrderEndpoints();
app.MapCustomerEndpoints();
app.MapReportEndpoints();
app.MapBlockingEndpoints();
app.MapPaymentEndpoints();
app.MapShippingEndpoints();
app.MapInventoryEndpoints();
app.MapExportEndpoints();
app.MapConfigurationEndpoints();
app.MapStartupEndpoints();
app.MapAuditEndpoints();
app.MapUploadEndpoints();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.Run();

public partial class Program;
