using MailCleanse.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMailCleanseInfrastructure();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    application = "Mail Cleanse",
    mode = "scan-only",
    status = "ready"
}));

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
