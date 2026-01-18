using campus.Implementation;
using commons.EventBase;
using commons.Protos;

namespace campus.Consumers;

[Envelope("RemoveFromRoom")]
public class RemoveFromRoomConsumer(
    ILogger<RemoveFromRoomConsumer> logger,
    CampusServiceImplementation implementation
    ) : CampusConsumer(logger), IConsumerDefinition
{
    private readonly CampusServiceImplementation _impl = implementation;
    public static object Example => new
    {
        Id = "Old Campus Student's Id"
    };

    protected override async Task HandleMessage(MessageBody body)
    {
        var userId = body.GetString("Id");

        await _impl.RemoveStudentFromRoom(userId);
        _logger.LogInformation("Assign Students to room finished successfully");
    }
}