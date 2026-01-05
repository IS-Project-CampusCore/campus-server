using commons.Protos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace commons.RequestBase;

public sealed class EmptyResponse()
{
    public static readonly EmptyResponse EmptyValue = new();

    public override string ToString() => "Empty";
}

public abstract class CampusMessage<TReq, TRes>(ILogger logger) : CampusMessageBase<TReq, TRes>(logger) where TReq : IRequestBase
{
    protected sealed override async Task<TRes> HandleInternal(TReq request, CancellationToken cancellationToken)
    {
        TRes res = await HandleMessage(request, cancellationToken);
        return res;
    }

    protected abstract Task<TRes> HandleMessage(TReq request, CancellationToken cancellationToken);
}

public abstract class CampusMessage<TReq>(ILogger logger) : CampusMessageBase<TReq, EmptyResponse>(logger) where TReq : IRequestBase
{
    protected sealed override async Task<EmptyResponse> HandleInternal(TReq request, CancellationToken cancellationToken)
    {
        await HandleMessage(request, cancellationToken);
        return EmptyResponse.EmptyValue;
    }
    protected abstract Task HandleMessage(TReq request, CancellationToken cancellationToken);
}

public abstract class CampusMessageBase<TReq, TRes>(
    ILogger logger
) : IRequestHandler<TReq, MessageResponse> where TReq : IRequestBase
{
    protected readonly ILogger _logger = logger;
    public async Task<MessageResponse> Handle(TReq request, CancellationToken cancellationToken)
    {
        string handlerName = this.GetType().Name;
        string requestName = typeof(TReq).Name;

        _logger.LogInformation("[{Handler}] Started message request.", handlerName);

        string? validationError = request.Validate();
        if (!string.IsNullOrEmpty(validationError))
        {
            _logger.LogError("[{Handler}] Validation failed for {Request}: {Error}.", handlerName, requestName, validationError);
            return MessageResponse.BadRequest(validationError);
        }

        try
        {
            TRes res = await HandleInternal(request, cancellationToken);
            return MessageResponse.Ok(res is EmptyResponse ? null : res);
        }
        catch (ServiceMessageException ex)
        {
            _logger.LogInformation("[{Handler}] Handled Service Exception:{ExName}, Msg:{Msg}.", handlerName, ex.GetType().Name, ex.Message);
            return ex.ToResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError("[{Handler}] Unhandled Exception:{ExName}, Msg:{Msg}.", handlerName, ex.GetType().Name, ex.Message);
            return MessageResponse.Error(ex);
        }
    }

    protected abstract Task<TRes> HandleInternal(TReq request, CancellationToken cancellationToken);
}
