using commons.Protos;
using commons.RequestBase;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace commons.EventBase;

public sealed class EmptyResponse()
{
    public static readonly EmptyResponse EmptyValue = new();

    public override string ToString() => "Empty";
}

public abstract class CampusConsumer<TResponse>(ILogger logger) : CampusConsumerBase<TResponse>(logger)
{
    protected sealed override async Task<TResponse> HandleInternal(Protos.MessageBody body) 
        => await HandleMessage(body);

    protected abstract Task<TResponse> HandleMessage(Protos.MessageBody body);
}

public abstract class CampusConsumer(ILogger logger) : CampusConsumerBase<EmptyResponse>(logger)
{
    protected sealed override async Task<EmptyResponse> HandleInternal(Protos.MessageBody body)
    {
        await HandleMessage(body);
        return EmptyResponse.EmptyValue;
    }

    protected abstract Task HandleMessage(Protos.MessageBody body);
}

public abstract class CampusConsumerBase<TResponse>(
    ILogger logger) : IConsumer<Envelope>
{
    protected readonly ILogger _logger = logger;
    public async Task Consume(ConsumeContext<Envelope> context)
    {
        bool success = false;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = context.CorrelationId ?? Guid.Empty,
            ["RequestId"] = context.RequestId ?? Guid.Empty,
            ["EventType"] = context.Message.EventType,
            ["MessageId"] = context.MessageId ?? Guid.Empty,
        }))
        {
            _logger.LogInformation("Starting consuming event.");
            try
            {
                TResponse result = await HandleInternal(new Protos.MessageBody(context.Message.Payload));

                if (context.RequestId.HasValue && result is not EmptyResponse)
                {
                    MessageResponse response = MessageResponse.Ok(result);
                    await context.RespondAsync(response);
                }

                success = true;
            }
            catch (EventException ex)
            {
                _logger.LogWarning(ex, "Handled Event Exception:{ExName}, Msg:{Msg}.", ex.GetType().Name, ex.Message);
                if (context.RequestId.HasValue)
                {
                    await context.RespondAsync(ex.ToResponse());
                }
                else
                    throw;
            }
            catch (ServiceMessageException ex)
            {
                _logger.LogWarning(ex, "Handled Service Exception:{ExName}, Msg:{Msg}.", ex.GetType().Name, ex.Message);
                if (context.RequestId.HasValue)
                {
                    await context.RespondAsync(ex.ToResponse());
                }
                else
                    throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled Exception:{ExName}, Msg:{Msg}.", ex.GetType().Name, ex.Message);
                if (context.RequestId.HasValue)
                {
                    await context.RespondAsync(MessageResponse.Error(ex));
                }
                else
                    throw;
            }
            finally
            {
                _logger.LogInformation("Ending consuming event Result:{Result}", success ? "Success" : "Fail");
            }
        }
    }

    protected abstract Task<TResponse> HandleInternal(Protos.MessageBody body);
}
