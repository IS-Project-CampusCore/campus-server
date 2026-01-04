using Grpc.Core;
using commons;
using commons.Protos;
using chatServiceClient;
using MediatR;
using Chat.Implementation;

namespace Chat.Services;

public class CreateGroupMessage(
    ILogger<CreateGroupMessage> logger,
    ChatServiceImplementation implementation
) : IRequestHandler<CreateGroupRequest, MessageResponse>
{
    private readonly ChatServiceImplementation _impl = implementation;
    private readonly ILogger<CreateGroupMessage> _logger = logger;

    public async Task<MessageResponse> Handle(CreateGroupRequest request, CancellationToken token)
    {
        if (request is null || request.Name is null || request.AdminId is null)
        {
            _logger.LogError("Create Group request is empty");
            return MessageResponse.BadRequest("Empty request");
        }

        try
        {
            string groupId = await _impl.CreateGroup(request.Name, request.AdminId);

            return MessageResponse.Ok(groupId);
        }
        catch (ServiceMessageException ex)
        {
            return ex.ToResponse();
        }
        catch (Exception ex)
        {
            return MessageResponse.Error(ex.Message);
        }
    }
}
