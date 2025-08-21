namespace Ai.Tutor.Services.Mediation;

public sealed class Mediator(IServiceProvider services) : IMediator
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var handler = services.GetService(handlerType);
        if (handler is null)
        {
            throw new InvalidOperationException($"No handler registered for {request.GetType().Name} -> {typeof(TResponse).Name}");
        }

        // Invoke handler.Handle(request, ct)
        var method = handlerType.GetMethod("Handle");
        if (method is null)
        {
            throw new InvalidOperationException($"Handler {handler.GetType().Name} does not have a Handle method");
        }

        var result = method.Invoke(handler, [request, ct]);
        return (Task<TResponse>)result!;
    }

    public Task Send(IRequest request, CancellationToken ct = default)
    {
        var handlerType = typeof(IRequestHandler<>).MakeGenericType(request.GetType());
        var handler = services.GetService(handlerType);
        if (handler is null)
        {
            throw new InvalidOperationException($"No handler registered for {request.GetType().Name}");
        }

        var method = handlerType.GetMethod("Handle");
        if (method is null)
        {
            throw new InvalidOperationException($"Handler {handler.GetType().Name} does not have a Handle method");
        }

        var result = method.Invoke(handler, [request, ct]);
        return (Task)result!;
    }
}
