namespace MailCleanse.Infrastructure.Yahoo;

public sealed class YahooMailOptions
{
    public const string SectionName = "Mail:Yahoo";

    public string Host { get; init; } = "imap.mail.yahoo.com";
    public int Port { get; init; } = 993;
    public bool UseSsl { get; init; } = true;
    public string Username { get; init; } = string.Empty;
    public string AppPassword { get; init; } = string.Empty;
}
