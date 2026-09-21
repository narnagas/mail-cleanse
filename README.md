# Mail Cleanse

Mail Cleanse is a .NET application for safely analyzing and cleaning an email inbox, beginning with Yahoo Mail.

## Goals

- Connect securely to Yahoo Mail.
- Scan and classify messages without modifying the mailbox by default.
- Identify likely marketing and promotional email.
- Present cleanup candidates for review.
- Add archive/delete actions only after the review workflow is established.
- Keep credentials and secrets out of source control.

## Solution structure

- `src/MailCleanse.Api` — ASP.NET Core API and composition root.
- `src/MailCleanse.Core` — domain models, interfaces, and cleanup rules.
- `src/MailCleanse.Infrastructure` — Yahoo/IMAP integration and external services.
- `tests/MailCleanse.Tests` — automated tests.

## Safety model

The initial implementation is scan-only. Mailbox mutation will be introduced separately with explicit review and confirmation controls.

## Local development

Prerequisites:

- .NET 8 SDK

Clone the repository, then run:

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/MailCleanse.Api
```

Do not commit passwords, app passwords, OAuth tokens, or local secret files.
