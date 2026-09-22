using MailCleanse.Core.Abstractions;
using MailCleanse.Infrastructure;
using MailCleanse.Infrastructure.Yahoo;
using MailKit.Security;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

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

app.MapGet("/auth/yahoo", (IOptions<YahooOAuthOptions> options) =>
{
    var oauth = options.Value;

    if (string.IsNullOrWhiteSpace(oauth.ClientId))
        return Results.Problem("Yahoo OAuth ClientId is not configured.");

    if (string.IsNullOrWhiteSpace(oauth.RedirectUri))
        return Results.Problem("Yahoo OAuth RedirectUri is not configured.");

    var state = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
    var nonce = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
    var codeVerifier = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
    var challengeBytes = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
    var codeChallenge = WebEncoders.Base64UrlEncode(challengeBytes);

    // Temporary development-only storage for the next callback step.
    // This is intentionally process-local and will be replaced with a proper
    // short-lived OAuth state store before production use.
    YahooPkceState.Store(state, codeVerifier);

    var authorizationUrl = QueryHelpers.AddQueryString(
        oauth.AuthorizationEndpoint,
        new Dictionary<string, string?>
        {
            ["client_id"] = oauth.ClientId,
            ["redirect_uri"] = oauth.RedirectUri,
            ["response_type"] = "code",
            ["scope"] = oauth.Scope,
            ["state"] = state,
            ["nonce"] = nonce,
            ["code_challenge"] = codeChallenge,
            ["code_challenge_method"] = "S256"
        });

    return Results.Redirect(authorizationUrl);
});

app.MapGet("/auth/yahoo/callback", async (
    string? code,
    string? state,
    string? error,
    string? error_description,
    IOptions<YahooOAuthOptions> options,
    CancellationToken cancellationToken) =>
{
    if (!string.IsNullOrWhiteSpace(error))
    {
        return Results.BadRequest(new
        {
            authenticated = false,
            error,
            errorDescription = error_description
        });
    }

    if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
        return Results.BadRequest(new { authenticated = false, error = "Missing authorization code or state." });

    if (!YahooPkceState.TryTake(state, out var codeVerifier) ||
        string.IsNullOrWhiteSpace(codeVerifier))
    {
        return Results.BadRequest(new
        {
            authenticated = false,
            error = "OAuth state is invalid, expired, or has already been used."
        });
    }

    var oauth = options.Value;

    using var httpClient = new HttpClient();

    using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, oauth.TokenEndpoint)
    {
        Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = oauth.ClientId,
            ["redirect_uri"] = oauth.RedirectUri,
            ["code"] = code,
            ["code_verifier"] = codeVerifier
        })
    };

    using var tokenResponse = await httpClient.SendAsync(tokenRequest, cancellationToken);
    var tokenJson = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);

    if (!tokenResponse.IsSuccessStatusCode)
    {
        return Results.Json(
            new
            {
                authenticated = false,
                error = "Yahoo token exchange failed.",
                statusCode = (int)tokenResponse.StatusCode
            },
            statusCode: StatusCodes.Status502BadGateway);
    }

    using var document = JsonDocument.Parse(tokenJson);
    var root = document.RootElement;

    var accessTokenReceived =
        root.TryGetProperty("access_token", out var accessToken) &&
        !string.IsNullOrWhiteSpace(accessToken.GetString());

    var refreshTokenReceived =
        root.TryGetProperty("refresh_token", out var refreshToken) &&
        !string.IsNullOrWhiteSpace(refreshToken.GetString());

    var tokenType =
        root.TryGetProperty("token_type", out var tokenTypeElement)
            ? tokenTypeElement.GetString()
            : null;

    long? expiresIn = null;
    if (root.TryGetProperty("expires_in", out var expiresElement) &&
        expiresElement.TryGetInt64(out var seconds))
    {
        expiresIn = seconds;
    }

    return Results.Ok(new
    {
        authenticated = accessTokenReceived,
        tokenType,
        accessTokenReceived,
        refreshTokenReceived,
        expiresIn,
        note = "Tokens were received only for this diagnostic and were not logged or persisted."
    });
});

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


internal static class YahooPkceState
{
    private static readonly Dictionary<string, string> Values = new();
    private static readonly object Sync = new();

    public static void Store(string state, string codeVerifier)
    {
        lock (Sync)
        {
            Values[state] = codeVerifier;
        }
    }

    public static bool TryTake(string state, out string? codeVerifier)
    {
        lock (Sync)
        {
            if (!Values.Remove(state, out var value))
            {
                codeVerifier = null;
                return false;
            }

            codeVerifier = value;
            return true;
        }
    }
}
