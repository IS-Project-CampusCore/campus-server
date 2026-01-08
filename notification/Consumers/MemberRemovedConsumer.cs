using commons.EventBase;
using notification.Hubs;
using commons.SignalRBase;

namespace notification.Consumers;

[Envelope("MemberRemoved")]
public class MemberRemovedConsumer(
    ILogger<MemberRemovedConsumer> logger,
    INotifier<ChatHub> notifier,
    IConnectionMapping<ChatHub> connectionMapping
    ) : SignalRConsumer<ChatHub>(logger, notifier, connectionMapping), ISignalRDefinition
{
    public static object Example => new
    {
        GroupId = "Group's Id",
        MemberId = "Member's Id",
        MemberName = "Member's Name"
    };

    public static string Message => "RemoveMember";

    public static object Content => new
    {
        GroupId = "Group's Id",
        MemberId = "Member's Id"
    };

    protected override async Task HandleMessage(commons.Protos.MessageBody body)
    {
        string groupId = body.GetString("GroupId");
        string memberId = body.GetString("MemberId");
        string memberName = body.GetString("MemberName");

        await _notifier.RemoveUserFromGroupAsync(memberId, groupId);

        await _notifier.SendToGroupAsync(
            groupId,
            "RemoveMember",
            new
            {
                GroupId = groupId,
                MemberId = memberId,
            });

        _logger.LogInformation($"User:{memberName} ({memberId}) removed from Group:{groupId}");
    }
}