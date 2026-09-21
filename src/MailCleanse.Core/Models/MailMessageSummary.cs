namespace MailCleanse.Core.Models;

public sealed record MailMessageSummary(
    string Id,
    string From,
    string Subject,
    DateTimeOffset ReceivedAt,
    bool IsRead);
