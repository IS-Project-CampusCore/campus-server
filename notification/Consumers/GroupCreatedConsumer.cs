using commons.EventBase;
using commons.SignalRBase;
using Microsoft.AspNetCore.SignalR;
using notification.Hubs;

namespace notification.Consumers;

[Envelope("GroupCreated")]
public class GroupCreatedConsumer(
    ILogger<GroupCreatedConsumer> logger,
    INotifier<ChatHub> notifier,
    IConnectionMapping<ChatHub> connectionMapping
    ) : SignalRConsumer<ChatHub>(logger, notifier, connectionMapping), ISignalRDefinition
{
    public static object Example => new
    {
        GroupId = "Created group Id",
        GroupName = "Created group Name",
        AdminId = "Group creater Id"
    };

    public static string Message => "GroupCreated";
    public static object Content => new
    {
        GroupId = "Created group Id",
        GroupName = "Created group Name"
    };

    protected override async Task HandleMessage(commons.Protos.MessageBody body)
    {
        string groupId = body.GetString("GroupId");
        string groupName = body.GetString("GroupName");
        string adminId = body.GetString("AdminId");

        await _notifier.AddUserToGroupAsync(adminId, groupId);

        await _notifier.SendToGroupAsync(
            groupId,
            Message,
            new
            {
                GroupId = groupId,
                GroupName = groupName
            });

        _logger.LogInformation($"Admin:{adminId} connected to new Group:{groupName} ({groupId})");
    }
}
