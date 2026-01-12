using commons.EventBase;
using commons.SignalRBase;
using notification.Hubs;

namespace notification.Consumers;

[Envelope("MemberLeft")]
public class MemberLeftConsumer(
    ILogger<MemberLeftConsumer> logger,
    INotifier<ChatHub> notifier,
    IConnectionMapping<ChatHub> connectionMapping
    ) : SignalRConsumer<ChatHub>(logger, notifier, connectionMapping), ISignalRDefinition
{
    public static object Example => new
    {
        GroupId = "Group's Id",
        MemberId = "Member's Id",
        MemberName = "Member's Name",
        NewAdminId = "Group's new Admin Id"
    };

    public static string Message => "MemberLeft";

    public static object Content => new
    {
        GroupId = "Group's Id",
        MemberId = "Member's Id",
        MemberName = "Member's Name",
        NewAdminId = "Group's new Admin Id"
    };

    protected override async Task HandleMessage(commons.Protos.MessageBody body)
    {
        string groupId = body.GetString("GroupId");
        string memberId = body.GetString("MemberId");
        string memberName = body.GetString("MemberName");
        string newAdminId = body.GetString("NewAdminId");

        await _notifier.RemoveUserFromGroupAsync(memberId, groupId);

        await _notifier.SendToGroupAsync(
            groupId,
            Message,
            new
            {
                GroupId = groupId,
                MemberId = memberId,
                MemberName = memberName,
                NewAdminId = newAdminId
            });

        _logger.LogInformation($"User:{memberName} ({memberId}) left Group:{groupId}");
    }
}
