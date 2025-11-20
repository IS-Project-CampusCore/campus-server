using commons;
using commons.Protos;
using Grpc.Core;
using MediatR;
using users.Model;
using usersServiceClient;

namespace users.Services;

public class RegisterMessage(
    ILogger<LoginMessage> logger,
    IUsersServiceImplementation implementation
) : IRequestHandler<RegisterRequest, MessageResponse>
{
    private readonly ILogger<LoginMessage> _logger = logger;
    private readonly IUsersServiceImplementation _implementation = implementation;

    public Task<MessageResponse> Handle(RegisterRequest request, CancellationToken token)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Role))
        {
            _logger.LogError("Request filed can not be empty");
            throw new BadRequestException("Request filed can not be empty");
        }

        try
        {
            var email = request.Email;
            var name = request.Name;
            var role = User.StringToRole(request.Role);

            return Task.FromResult(MessageResponse.Ok(_implementation.RegisterUser(email, name, role)));
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