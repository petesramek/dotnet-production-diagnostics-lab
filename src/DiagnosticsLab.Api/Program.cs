using DiagnosticsLab.Api.Configuration;
using DiagnosticsLab.Api.Data;
using DiagnosticsLab.Api.Endpoints;
using DiagnosticsLab.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Scenario 09: Configuration validation
builder.Services
    .AddOptions<ExternalServicesOptions>()
    .Bind(builder.Configuration.GetSection(ExternalServicesOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(options => Uri.TryCreate(options.BillingApiBaseUrl, UriKind.Absolute, out _), "Billing API base URL must be an absolute URI.")
    .ValidateOnStart();

// Scenario 01 & 05: Data access (slow queries, N+1)
builder.Services.AddDbContext<AppDbContext>(options => {
    var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=diagnostics-lab.db";
    options.UseSqlite(connectionString);
});

// Scenario 04 & 07: External dependency simulation
builder.Services.AddScoped<FakeShippingClient>();
builder.Services.AddScoped<FakeInventoryClient>();

// Scenario 11: Startup failure simulation
builder.Services.AddSingleton<FakeStartupDependency>();

// Scenario 12: Logging failure simulation
builder.Services.AddSingleton<FakeAuditSink>();

// Scenario 14: HttpClient usage
builder.Services.AddHttpClient();

// Scenario 03 & 13: Request context access
builder.Services.AddHttpContextAccessor();

// Scenario 17: Cache behavior
builder.Services.AddMemoryCache();

// Scenario 10: Health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("Application is running."));

var app = builder.Build();

await DataSeeder.SeedAsync(app.Services);

app.MapGet("/", () => Results.Ok(new {
    Name = ".NET Production Diagnostics Lab",
    Description = "Small ASP.NET Core lab for production-style diagnostics scenarios."
}));

// Scenario 01: Excessive Data Materialization
app.MapExcessiveDataMaterializationEndpoints();

// Scenario 02: Missing cancellation / timeouts
app.MapMissingCancellationPropagationEndpoints();

// Scenario 03: Missing Log Context
app.MapMissingLogContextEndpoints();

// Scenario 04: Missing Dependency Timeouts
app.MapMissingDependencyTimeoutsEndpoints();

// Scenario 05: N+1 queries
app.MapNPlusOneQueryEndpoints();

// Scenario 06: Blocking request handling
app.MapBlockingRequestHandlingEndpoints();

// Scenario 07: Retry storms
app.MapUnboundedRetriesEndpoints();

// Scenario 08: Large response buffering vs streaming
app.MapLargeResponseEndpoints();

// Scenario 09: Configuration validation
app.MapInvalidConfigurationEndpoints();

// Scenario 10: Health checks
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

// Scenario 11: Startup failure handling
app.MapSilentStartupFailureEndpoints();

// Scenario 12: Logging failure isolation
app.MapLoggingFailureEndpoints();

// Scenario 13: Request body memory pressure
app.MapRequestBodyMemoryPressureEndpoints();

// Scenario 14: Socket exhaustion
app.MapSocketExhaustionEndpoints();

// Scenario 15: ThreadPool starvation
app.MapThreadPoolStarvationEndpoints();

// Scenario 16: LOH fragmentation
app.MapLohFragmentationEndpoints();

// Scenario 17: Cache stampede
app.MapCacheStampedeEndpoints();

// Scenario 18: Native AOT
app.MapNativeAotEndpoints();

app.Run();

public partial class Program;