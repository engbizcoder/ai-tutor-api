namespace Ai.Tutor.Api.Services;

using Ai.Tutor.Services.Services;
using Microsoft.Extensions.DependencyInjection;

internal static class ServicesRegistrationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IOrgDeletionService, OrgDeletionService>();
        services.AddScoped<IUserDeletionService, UserDeletionService>();
        services.AddHostedService<OrgPurgeBackgroundService>();
        return services;
    }
}
