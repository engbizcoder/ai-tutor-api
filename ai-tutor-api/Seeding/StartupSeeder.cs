namespace Ai.Tutor.Api.Seeding;

using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;

public sealed class StartupSeeder(IServiceProvider services) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var orgs = scope.ServiceProvider.GetRequiredService<IOrgRepository>();
        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var members = scope.ServiceProvider.GetRequiredService<IOrgMemberRepository>();
        var threads = scope.ServiceProvider.GetRequiredService<IThreadRepository>();
        var messages = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
        var files = scope.ServiceProvider.GetRequiredService<IFileRepository>();
        var attachments = scope.ServiceProvider.GetRequiredService<IAttachmentRepository>();
        var references = scope.ServiceProvider.GetRequiredService<IReferenceRepository>();
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorageAdapter>();

        // Create/find demo org and user
        var org = await orgs.GetBySlugAsync("demo", cancellationToken);
        if (org is null)
        {
            org = new Org
            {
                Name = "Demo Org",
                Slug = "demo",
                Type = OrgType.Business,
            };
            org = await orgs.AddAsync(org, cancellationToken);
        }

        var user = await users.GetByEmailAsync("demo@example.com", cancellationToken);
        if (user is null)
        {
            user = new User
            {
                PrimaryOrgId = org.Id,
                Name = "Demo User",
                Email = "demo@example.com",
            };
            user = await users.AddAsync(user, cancellationToken);

            // Ensure membership
            var isMember = await members.IsMemberAsync(org.Id, user.Id, cancellationToken);
            if (!isMember)
            {
                await members.AddAsync(
                    new OrgMember { OrgId = org.Id, UserId = user.Id, Role = OrgRole.Member, JoinedAt = DateTime.UtcNow },
                    cancellationToken);
            }
        }

        // If the demo user already has any threads, skip seeding
        var existing = await threads.ListByUserAsync(org.Id, user.Id, cancellationToken);
        if (existing.Count > 0)
        {
            return;
        }

        // Create a sample thread
        var thread = new ChatThread
        {
            OrgId = org.Id,
            UserId = user.Id,
            Title = "Welcome to AI Maths Tutor",
            Status = ChatThreadStatus.Active,
            SortOrder = 1000m,
        };
        thread = await threads.AddAsync(thread, cancellationToken);

        // Two sample messages
        var m1 = new ChatMessage
        {
            ThreadId = thread.Id,
            SenderType = SenderType.User,
            SenderId = user.Id,
            Status = MessageStatus.Sent,
            Content = "Hi tutor, can you help me understand Pythagoras' theorem?",
        };
        await messages.AddAsync(m1, cancellationToken);

        var m2 = new ChatMessage
        {
            ThreadId = thread.Id,
            SenderType = SenderType.Ai,
            SenderId = null,
            Status = MessageStatus.Sent,
            Content = "Of course! In a right-angled triangle, a^2 + b^2 = c^2 where c is the hypotenuse. Want to try an example?",
        };
        await messages.AddAsync(m2, cancellationToken);

        // Seed a sample file, attachment, and reference
        // Create a small dummy PDF file in storage
        var demoBytes = System.Text.Encoding.UTF8.GetBytes("%PDF-1.4\n% Demo PDF content for seeding\n");
        await using var demoStream = new MemoryStream(demoBytes, writable: false);
        var storageKey = await storage.UploadAsync(demoStream, "demo.pdf", "application/pdf", cancellationToken);
        var storageUrl = await storage.GetPresignedUrlAsync(storageKey, TimeSpan.FromHours(1), cancellationToken);

        var storedFile = new StoredFile
        {
            OrgId = org.Id,
            OwnerUserId = user.Id,
            FileName = "demo.pdf",
            ContentType = "application/pdf",
            StorageKey = storageKey,
            StorageUrl = storageUrl,
            SizeBytes = demoBytes.LongLength,
            Pages = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        storedFile = await files.AddAsync(storedFile, cancellationToken);

        // Create an attachment linking the file to the first user message
        var attachment = new Attachment
        {
            MessageId = m1.Id,
            FileId = storedFile.Id,
            Type = AttachmentType.Document,
            CreatedAt = DateTime.UtcNow,
        };
        await attachments.AddAsync(attachment, cancellationToken);

        // Create a reference pointing to the file within the thread
        var reference = new Reference
        {
            ThreadId = thread.Id,
            MessageId = m1.Id,
            Type = ReferenceType.File,
            Title = "Pythagoras Theorem PDF",
            FileId = storedFile.Id,
            CreatedAt = DateTime.UtcNow,
        };
        reference.Validate();
        await references.AddAsync(reference, org.Id, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
