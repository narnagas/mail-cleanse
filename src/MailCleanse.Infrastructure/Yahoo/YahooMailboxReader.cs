using MailCleanse.Core.Abstractions;
using MailCleanse.Core.Models;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using Microsoft.Extensions.Options;

namespace MailCleanse.Infrastructure.Yahoo;

public sealed class YahooMailboxReader(IOptions<YahooMailOptions> options) : IMailboxReader
{
    private readonly YahooMailOptions _options = options.Value;

    public async Task<IReadOnlyList<MailMessageSummary>> ScanAsync(
        int maxMessages,
        CancellationToken cancellationToken = default)
    {
        if (maxMessages is < 1 or > 500)
            throw new ArgumentOutOfRangeException(nameof(maxMessages), "Use a value from 1 to 500.");

        ValidateConfiguration();

        using var client = new ImapClient();

        await client.ConnectAsync(
            _options.Host,
            _options.Port,
            _options.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls,
            cancellationToken);

        await client.AuthenticateAsync(
            _options.Username,
            _options.AppPassword,
            cancellationToken);

        var inbox = client.Inbox;
        await inbox.OpenAsync(FolderAccess.ReadOnly, cancellationToken);

        if (inbox.Count == 0)
        {
            await client.DisconnectAsync(true, cancellationToken);
            return [];
        }

        var start = Math.Max(0, inbox.Count - maxMessages);
        var summaries = await inbox.FetchAsync(
            start,
            inbox.Count - 1,
            MessageSummaryItems.UniqueId |
            MessageSummaryItems.Envelope |
            MessageSummaryItems.Flags |
            MessageSummaryItems.InternalDate,
            cancellationToken);

        var messages = summaries
            .OrderByDescending(x => x.InternalDate)
            .Select(x => new MailMessageSummary(
                x.UniqueId.Id.ToString(),
                x.Envelope?.From?.ToString() ?? string.Empty,
                x.Envelope?.Subject ?? string.Empty,
                x.InternalDate ?? DateTimeOffset.MinValue,
                x.Flags?.HasFlag(MessageFlags.Seen) == true))
            .ToArray();

        await client.DisconnectAsync(true, cancellationToken);
        return messages;
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.Username))
            throw new InvalidOperationException("Yahoo username is not configured.");

        if (string.IsNullOrWhiteSpace(_options.AppPassword))
            throw new InvalidOperationException("Yahoo app password is not configured.");
    }
}
