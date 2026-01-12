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
        GroupId = "Group's Id",
        SenderName = "Member's Name",
        Content = "Text Message",
        FilesId = new [] {"File 1 Id", "File 2 Id" },
        SentAt = "Time"
    };

    public static string Message => "NewMessage";
    public static object Content => new
    {
        From = "Sender Id",
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
                From = senderId,
                Content = content,
                Files = filesId,
                At = sentAt,
            });

        await _implementation.SendEmailToOfflineMembers(
            groupId,
            [senderId],
            "NewMessage",
            JsonSerializer.Serialize(new
            {
                Sender_Name = senderName,
                Sent_At = sentAt.ToString("dd MMM yyyy, HH:mm"),
                Content = content,
                Files_Count = filesId?.Count().ToString() ?? "0"
            }));
    }
}