using Microsoft.Extensions.DependencyInjection;

namespace MailCleanse.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMailCleanseInfrastructure(this IServiceCollection services)
    {
        // Yahoo/IMAP services will be registered here.
        return services;
    }
}
