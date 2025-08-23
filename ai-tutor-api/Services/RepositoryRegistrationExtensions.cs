namespace Ai.Tutor.Api.Services;

using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

public static class RepositoryRegistrationExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IOrgRepository, OrgRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrgMemberRepository, OrgMemberRepository>();
        services.AddScoped<IFolderRepository, FolderRepository>();
        services.AddScoped<IThreadRepository, ThreadRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        return services;
    }
}
