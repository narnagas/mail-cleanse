using MailCleanse.Core.Abstractions;
using MailCleanse.Infrastructure.Yahoo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MailCleanse.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMailCleanseInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<YahooMailOptions>()
            .Bind(configuration.GetSection(YahooMailOptions.SectionName));

        services.AddScoped<IMailboxReader, YahooMailboxReader>();

        return services;
    }
}
