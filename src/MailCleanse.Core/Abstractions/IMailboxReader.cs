using MailCleanse.Core.Models;

namespace MailCleanse.Core.Abstractions;

public interface IMailboxReader
{
    Task<IReadOnlyList<MailMessageSummary>> ScanAsync(
        int maxMessages,
        CancellationToken cancellationToken = default);
}
