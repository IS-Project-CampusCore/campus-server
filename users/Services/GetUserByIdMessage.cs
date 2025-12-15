using commons;
using commons.Protos;
using MediatR;
using usersServiceClient;

namespace users.Services;

public class GetUserByIdMessage(
    ILogger<GetUserByIdMessage> logger,
    IUsersServiceImplementation implementation
) : IRequestHandler<UserIdRequest, MessageResponse>
{
    private readonly ILogger<GetUserByIdMessage> _logger = logger;
    private readonly IUsersServiceImplementation _implementation = implementation;

    public Task<MessageResponse> Handle(UserIdRequest request, CancellationToken token)
    {
        if (string.IsNullOrEmpty(request.Id))
        {
            _logger.LogError("ID can not be empty");
            throw new BadRequestException("ID can not be empty");
        }

        try
        {
            var id = request.Id;

            return Task.FromResult(MessageResponse.Ok(_implementation.GetUserById(id)));
        }
        catch (BadRequestException ex)
        {
            return Task.FromResult(MessageResponse.BadRequest(ex.Message));
        }
        catch (Exception ex)
        {
            return Task.FromResult(MessageResponse.Error(ex.Message));
        }
    }
}
