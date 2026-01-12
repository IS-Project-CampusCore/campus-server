using commons.Protos;
using MassTransit;
using MassTransit.Clients;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace commons.EventBase;

public interface IScopedMessagePublisher
{
    Task Publish<T>(string eventName, T message, CancellationToken cancellationToken = default) where T : class;

    Task<MessageResponse> SendAsync<T>(string eventName, T request, CancellationToken cancellationToken = default) where T : class;
}

public class ScopedMessagePublisher(IServiceScopeFactory scopeFactory) : IScopedMessagePublisher
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    public async Task Publish<T>(string eventName, T message, CancellationToken cancellationToken = default) where T : class
    {
        using var scope = _scopeFactory.CreateScope();
        var sendEndpointProvider = scope.ServiceProvider.GetRequiredService<ISendEndpointProvider>();

        var body = Protos.MessageBody.From(message);
        var envelope = new Envelope(eventName, body);

        var destinationAddress = new Uri("exchange:commons.EventBase:Envelope?type=direct");
        var endpoint = await sendEndpointProvider.GetSendEndpoint(destinationAddress);

        await endpoint.Send(envelope, context => context.SetRoutingKey(eventName), cancellationToken);
    }

    public async Task<MessageResponse> SendAsync<T>(string eventName, T request, CancellationToken cancellationToken = default) where T : class
    {
        using var scope = _scopeFactory.CreateScope();
        var clientFactory = scope.ServiceProvider.GetRequiredService<IClientFactory>();

        var body = Protos.MessageBody.From(request);
        var envelope = new Envelope(eventName, body);

        var destinationAddress = new Uri("exchange:commons.EventBase:Envelope?type=direct");

        var client = clientFactory.CreateRequestClient<Envelope>(destinationAddress);

        var response = await client.GetResponse<MessageResponse>(
            envelope,
            configurator =>
            {
                configurator.UseExecute(context => context.SetRoutingKey(eventName));
            },
            cancellationToken);
        return response.Message;
    }
}
