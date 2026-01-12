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
        => await HandleMessage(request, cancellationToken);

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
        bool success = false;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["Handler"] = this.GetType().Name,
            ["RequestType"] = typeof(TReq).Name,
        }))
        {
            _logger.LogInformation("Starting message request.");

            string? validationError = request.Validate();
            if (!string.IsNullOrEmpty(validationError))
            {
                _logger.LogError("Validation failed: {Error}.", validationError);
                return MessageResponse.BadRequest(validationError);
            }

            try
            {
                TRes res = await HandleInternal(request, cancellationToken);

                success = true;
                return MessageResponse.Ok(res is EmptyResponse ? null : res);
            }
            catch (ServiceMessageException ex)
            {
                _logger.LogWarning(ex, "Handled Service Exception:{ExName}, Msg:{Msg}.", ex.GetType().Name, ex.Message);
                return ex.ToResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled Exception:{ExName}, Msg:{Msg}.", ex.GetType().Name, ex.Message);
                return MessageResponse.Error(ex);
            }
            finally
            {
                _logger.LogInformation("Ending request message Result:{Result}", success ? "Success" : "Fail");
            }
        }
    }

    protected abstract Task<TRes> HandleInternal(TReq request, CancellationToken cancellationToken);
}
