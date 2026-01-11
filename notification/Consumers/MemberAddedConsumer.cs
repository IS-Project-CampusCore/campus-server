using commons.EventBase;
using notification.Hubs;
using commons.SignalRBase;

namespace notification.Consumers;

[Envelope("MemberAdded")]
public class MemberAddedConsumer(
    ILogger<MemberAddedConsumer> logger,
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

    public static string Message => "NewMember";
    public static object Content => new
    {
        GroupId = "Group's Id",
        MemberId = "Member's Id",
        MemberName = "Member's Name"
    };

    protected override async Task HandleMessage(commons.Protos.MessageBody body)
    {
        string groupId = body.GetString("GroupId");
        string memberId = body.GetString("MemberId");
        string memberName = body.GetString("MemberName");

        await _notifier.AddUserToGroupAsync(memberId, groupId);

        await _notifier.SendToGroupAsync(
            groupId,
            Message,
            new
            {
                GroupId = groupId,
                MemberId = memberId,
                MemberName = memberName
            });

        _logger.LogInformation($"User:{memberName} ({memberId}) added to Group:{groupId}");
    }
}
