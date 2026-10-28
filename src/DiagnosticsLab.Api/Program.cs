using DiagnosticsLab.Api.Data;
using DiagnosticsLab.Api.Scenarios;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddScenarios(builder.Configuration);

var app = builder.Build();

await DataSeeder.SeedAsync(app.Services);

app.MapGet("/", () => Results.Ok(new {
    Name = ".NET Production Diagnostics Lab",
    Description = "Small ASP.NET Core lab for production-style diagnostics scenarios."
}));

app.MapScenarios();

app.Run();

public partial class Program;
