# Mail Cleanse

Mail Cleanse is a .NET email-management application designed to help users securely connect their Yahoo Mail account and clean, organize, and manage messages across their Inbox and other mail folders.

Users authorize their own Yahoo account through OAuth 2.0. Mail Cleanse does not collect or store Yahoo account passwords. After authorization, the application can analyze mailbox content, identify messages such as marketing and promotional email, and present cleanup candidates for user review.

The project is being developed with a safety-first workflow. Initial mailbox access is read-only while scanning and classification are validated. Archive, move, and delete capabilities will be introduced with explicit user review and control before mailbox content is modified.

## Goals

- Connect users securely to Yahoo Mail through OAuth 2.0 with PKCE.
- Access Yahoo Mail through standard IMAP using OAuth authorization.
- Scan the Inbox and other user-selected mail folders.
- Analyze and classify messages without modifying the mailbox by default.
- Identify likely marketing, promotional, and other unwanted email.
- Present cleanup candidates for user review.
- Allow users to organize, archive, move, or delete selected messages.
- Keep the user in control of mailbox-changing actions.
- Store OAuth credentials securely and keep secrets out of source control.
- Support account disconnection and authorization revocation.

## Current development status

Yahoo OAuth 2.0 Authorization Code flow with PKCE has been implemented and validated for the permissions currently available to the application. Yahoo authentication, passkey verification, consent, authorization-code exchange, access-token issuance, and refresh-token issuance have been successfully tested.

Yahoo Mail IMAP OAuth permissions require separate approval from Yahoo and are not available through the normal self-service developer console. The project is therefore currently awaiting Yahoo Mail developer-access approval before OAuth-authenticated IMAP scanning can be completed.

The current implementation does not persist OAuth tokens during the diagnostic authorization flow.

## Planned workflow

```text
Yahoo account
     |
     v
OAuth 2.0 + PKCE authorization
     |
     v
Secure Yahoo IMAP connection
     |
     v
Scan Inbox / selected folders
     |
     v
Analyze and classify messages
     |
     v
User review
     |
     v
Archive / move / delete selected messages
```

## Solution structure

- `src/MailCleanse.Api` — ASP.NET Core API, OAuth endpoints, and application composition root.
- `src/MailCleanse.Core` — domain models, interfaces, and cleanup rules.
- `src/MailCleanse.Infrastructure` — Yahoo OAuth/IMAP integration and external services.
- `tests/MailCleanse.Tests` — automated tests.

## Safety and privacy model

Mail Cleanse is designed around user authorization and user control.

- Yahoo passwords are not collected or stored by Mail Cleanse.
- Authentication is performed by Yahoo using OAuth 2.0.
- OAuth PKCE is used for the authorization flow.
- Secrets and OAuth tokens must never be committed to source control.
- Initial mailbox processing is read-only.
- Destructive mailbox actions are separated from scanning and classification.
- Users review cleanup candidates before destructive actions are performed.
- Only the mailbox permissions necessary for the application's functionality should be requested.

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

Application configuration and credentials used for local development should be stored using .NET User Secrets or another appropriate secure configuration mechanism.

Do not commit passwords, OAuth access tokens, refresh tokens, client secrets, passkeys, or local secret files.

## Project status

Mail Cleanse is under active development. The current milestone is obtaining Yahoo developer authorization for Mail service access and then completing OAuth-authenticated IMAP integration.
