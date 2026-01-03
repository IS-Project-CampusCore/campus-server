using commons.Events;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using notification.Hubs;

namespace notification.Implementation;

public class MessageCreatedConsumer(IHubContext<ChatHub> hubContext) : IConsumer<MessageCreatedEvent>
{
    private readonly IHubContext<ChatHub> _hubContext = hubContext;

    public async Task Consume(ConsumeContext<MessageCreatedEvent> context)
    {
        var message = context.Message;

        await _hubContext.Clients.Group(message.GroupId)
            .SendAsync("ReciveMessage", new
            {
                From = message.SenderId,
                Content = message.Content,
                Files = message.FilesId,
                Time = message.Timestamp
            });
    }
}
