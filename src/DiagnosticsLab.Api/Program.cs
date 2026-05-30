using DiagnosticsLab.Api.Data;
using DiagnosticsLab.Api.Endpoints;
using DiagnosticsLab.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=diagnostics-lab.db";
    options.UseSqlite(connectionString);
});

builder.Services.AddScoped<FakeShippingClient>();
builder.Services.AddScoped<FakeInventoryClient>();

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

app.Run();

public partial class Program;
