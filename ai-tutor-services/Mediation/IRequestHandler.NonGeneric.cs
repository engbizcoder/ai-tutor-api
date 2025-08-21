namespace Ai.Tutor.Services.Mediation;

public interface IRequestHandler<in TRequest>
    where TRequest : IRequest
{
    Task Handle(TRequest request, CancellationToken ct = default);
}
