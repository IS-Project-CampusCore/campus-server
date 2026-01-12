using Chat.Implementation;
using commons.EventBase;
using commons.Protos;

namespace chat.Consumers;

[Envelope("GroupDeleted")]
public class GroupDeletedCleanupConsumer(
    ILogger<GroupDeletedCleanupConsumer> logger,
    IChatService chatServiceImplementation
    ) : CampusConsumer(logger), IConsumerDefinition
{
    private readonly IChatService _impl = chatServiceImplementation;
    public static object Example => new
    {
        GroupId = "Deleted group Id",
        GroupName = "Deleted group Name",
        GroupMemberIds = "Deleted group Member Ids"
    };

    protected override async Task HandleMessage(MessageBody body)
    {
        string groupId = body.GetString("GroupId");

        await _impl.DeleteGroupCleanupAsync(groupId);
        _logger.LogInformation("Delete Group Cleanup finished successfully");
    }
}
