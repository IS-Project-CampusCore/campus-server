using commons.EventBase;
using notification.Hubs;
using commons.SignalRBase;
using chatServiceClient;
using emailServiceClient;
using Notification.Implementation;
using System.Text.Json;

namespace notification.Consumers;

[Envelope("MessageCreated")]
public class MessageCreatedConsumer(
    ILogger<MessageCreatedConsumer> logger,
    INotifier<ChatHub> notifier,
    IConnectionMapping<ChatHub> connectionMapping,
    ChatNotificationImplementation implementation
    ) : SignalRConsumer<ChatHub>(logger, notifier, connectionMapping), ISignalRDefinition
{
    public static object Example => new
    {
        SenderId = "Sender's Id",
        SenderName = "Member's Name",
        GroupId = "Group's Id",
        GroupName = "Group's Name",
        Content = "Text Message",
        FilesId = new [] {"File 1 Id", "File 2 Id" },
        SentAt = "Time"
    };

    public static string Message => "NewMessage";
    public static object Content => new
    {
        Group = "Group's Id",
        From = "Sender's Id",
        Content = "Text Content",
        Files = new[] { "File 1 Id", "File 2 Id" },
        At = "Time"
    };

    private readonly ChatNotificationImplementation _implementation = implementation;

    protected override async Task HandleMessage(commons.Protos.MessageBody body)
    {
        string senderId = body.GetString("SenderId");
        string senderName = body.GetString("SenderName");
        string groupId = body.GetString("GroupId");
        string groupName = body.GetString("GroupName");
        string? content = body.TryGetString("Content");
        var filesId = body.TryGetArray("FilesId")?.IterateStrings();

        DateTime sentAt;
        if (!DateTime.TryParse(body.TryGetString("SentAt"), out sentAt))
            sentAt = DateTime.UtcNow;

        await _notifier.SendToGroupAsync(
            groupId,
            Message,
            new
            {
                Group = groupId,
                From = senderId,
                Content = content,
                Files = filesId,
                At = sentAt,
            });

        string filesDisplay = (filesId != null && filesId.Any()) ? "style=\"display:table-row;\"" : "style=\"display:none;\"";
        string contentDisplay = !string.IsNullOrWhiteSpace(content) ? "style=\"display:table-row;\"" : "style=\"display:none;\"";

        string emailJsonData = JsonSerializer.Serialize(new
        {
            From = senderName,
            Group = groupName,
            Sent_At = sentAt.ToString("dd MMM yyyy, HH:mm"),
            Content = content,
            Content_Display = contentDisplay,
            Files_Display = filesDisplay,
            Files_Count = filesId?.Count().ToString() ?? "None"
        });

        await _implementation.SendBroadcastNotification(groupId, senderId, "NewMessage", emailJsonData);
    }
}