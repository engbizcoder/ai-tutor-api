namespace Ai.Tutor.Domain.Repositories;

public interface IUnitOfWork
{
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> work, CancellationToken ct = default);
}
