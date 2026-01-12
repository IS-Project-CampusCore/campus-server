using commons.EventBase;
using commons.SignalRBase;
using notification.Hubs;

namespace notification.Consumers;

[Envelope("GroupDeleted")]
public class GroupDeletedConsumer(
    ILogger<GroupDeletedConsumer> logger,
    INotifier<ChatHub> notifier,
    IConnectionMapping<ChatHub> connectionMapping
    ) : SignalRConsumer<ChatHub>(logger, notifier, connectionMapping), ISignalRDefinition
{
    public static object Example => new
    {
        GroupId = "Deleted group Id",
        GroupName = "Deleted group Name",
        GroupMemberIds = "Deleted group Member Ids"
    };

    public static string Message => "GroupDeleted";
    public static object Content => new
    {
        GroupId = "Deleted group Id",
        GroupName = "Deleted group Name"
    };

    protected override async Task HandleMessage(commons.Protos.MessageBody body)
    {
        string groupId = body.GetString("GroupId");
        string groupName = body.GetString("GroupName");
        IEnumerable<string> memberIds = body.GetArray("GroupMemberIds").IterateStrings();

        await _notifier.SendToGroupAsync(
            groupId,
            Message,
            new
            {
                GroupId = groupId,
                GroupName = groupName
            });

        _logger.LogInformation($"Group:{groupName} deleted");

        foreach (string memberId in memberIds)
        {
            await _notifier.RemoveUserFromGroupAsync(memberId, groupId);
        }
    }
}
