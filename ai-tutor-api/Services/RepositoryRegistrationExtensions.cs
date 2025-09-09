namespace Ai.Tutor.Api.Services;

using Domain.Repositories;
using Infrastructure.Repositories;
using Infrastructure.Storage;
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
        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();
        services.AddScoped<IReferenceRepository, ReferenceRepository>();
        services.AddScoped<IFileStorageAdapter, LocalFileStorageAdapter>();
        return services;
    }
}
