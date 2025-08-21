namespace Ai.Tutor.Services.Mediation;

public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default);

    Task Send(IRequest request, CancellationToken ct = default);
}
