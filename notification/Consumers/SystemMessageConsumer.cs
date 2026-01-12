using commons.EventBase;
using commons.SignalRBase;
using notification.Hubs;
using Notification.Implementation;
using System.Text.Json;

namespace notification.Consumers;

[Envelope("GroupSystemMessage")]
public class SystemMessageConsumer(
    ILogger<SystemMessageConsumer> logger,
    INotifier<ChatHub> notifier,
    IConnectionMapping<ChatHub> connectionMapping,
    ChatNotificationImplementation implementation
    ) : SignalRConsumer<ChatHub>(logger, notifier, connectionMapping), ISignalRDefinition
{
    public static object Example => new
    {
        SubjectId = "Subject's Id",
        GroupId = "Group's Id",
        GroupName = "Group's Name",
        PrivateContent = "Private Text Message or empty if no private message",
        PublicContent = "Public Text Message",
        SentAt = "Time",
        IsBroadcast = "Broadcast"
    };

    public static string Message => "SystemMessage";
    public static object Content => new
    {
        From = "Sender Id",
        PublicContent = "Public Text Message",
        At = "Time"
    };

    private readonly ChatNotificationImplementation _implementation = implementation;

    protected override async Task HandleMessage(commons.Protos.MessageBody body)
    {
        string subjectId = body.GetString("SubjectId");
        string groupId = body.GetString("GroupId");
        string groupName = body.GetString("GroupName");
        string publicContent = body.GetString("PublicContent");
        string privateContent = body.GetString("PrivateContent");
        bool isBroadcast = body.GetBool("IsBroadcast");

        DateTime sentAt;
        if (!DateTime.TryParse(body.TryGetString("SentAt"), out sentAt))
            sentAt = DateTime.UtcNow;

        await _notifier.SendToGroupAsync(
            groupId,
            "SystemMessage",
            new
            {
                From = subjectId,
                PublicContent = publicContent,
                At = sentAt,
            });

        string emailJsonData = JsonSerializer.Serialize(new
        {
            Group_Name = groupName,
            Sent_At = sentAt.ToString("dd MMM yyyy, HH:mm"),
            Content = isBroadcast ? publicContent : privateContent
        });

        if (isBroadcast)
        {
            await _implementation.SendBroadcastNotification(groupId, subjectId, "SystemMessage", emailJsonData);
        }
        else if (!string.IsNullOrEmpty(privateContent))
        {
            await _implementation.SendTargetedNotification(groupId, subjectId, "SystemMessage", emailJsonData);
        }
    }
}
