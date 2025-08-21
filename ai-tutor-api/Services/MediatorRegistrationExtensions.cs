namespace Ai.Tutor.Api.Services;

using System.Reflection;
using Ai.Tutor.Services.Mediation;
using Microsoft.Extensions.DependencyInjection;

public static class MediatorRegistrationExtensions
{
    public static IServiceCollection AddMediatorHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface)
            {
                continue;
            }

            foreach (var iface in type.GetInterfaces())
            {
                if (!iface.IsGenericType)
                {
                    continue;
                }

                var def = iface.GetGenericTypeDefinition();
                if (def == typeof(IRequestHandler<>) || def == typeof(IRequestHandler<,>))
                {
                    services.AddScoped(iface, type);
                }
            }
        }

        return services;
    }
}
