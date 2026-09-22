namespace MailCleanse.Infrastructure.Yahoo;

public sealed class YahooOAuthOptions
{
    public const string SectionName = "Mail:Yahoo:OAuth";

    public string ClientId { get; init; } = string.Empty;
    public string RedirectUri { get; init; } = string.Empty;
    public string AuthorizationEndpoint { get; init; } = "https://api.login.yahoo.com/oauth2/request_auth";
    public string TokenEndpoint { get; init; } = "https://api.login.yahoo.com/oauth2/get_token";
    public string Scope { get; init; } = "openid email profile";
}
