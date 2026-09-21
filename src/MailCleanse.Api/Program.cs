using MailCleanse.Core.Abstractions;
using MailCleanse.Infrastructure;
using MailKit.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMailCleanseInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    application = "Mail Cleanse",
    provider = "Yahoo",
    mode = "scan-only",
    status = "ready"
}));

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.MapGet("/api/mail/scan", async (
    int? limit,
    IMailboxReader mailbox,
    CancellationToken cancellationToken) =>
{
    var maxMessages = limit ?? 50;

    if (maxMessages is < 1 or > 500)
        return Results.BadRequest(new { error = "limit must be between 1 and 500." });

    try
    {
        var messages = await mailbox.ScanAsync(maxMessages, cancellationToken);

        return Results.Ok(new
        {
            count = messages.Count,
            scanOnly = true,
            messages
        });
    }
    catch (AuthenticationException)
    {
        return Results.Json(
            new
            {
                error = "Yahoo authentication failed.",
                detail = "Check the server log for the authentication mechanisms advertised by Yahoo. The password is never logged."
            },
            statusCode: StatusCodes.Status401Unauthorized);
    }
});

app.Run();
