namespace Ai.Tutor.Infrastructure.Data;

using Ai.Tutor.Domain.Repositories;

public sealed class EfUnitOfWork(AiTutorDbContext db) : IUnitOfWork
{
    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> work, CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        try
        {
            await work(ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}
