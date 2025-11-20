using commons;
using commons.Protos;
using MediatR;
using usersServiceClient;

namespace users.Services;

public class LoginMessage(
    ILogger<LoginMessage> logger,
    IUsersServiceImplementation implementation
) : IRequestHandler<LoginRequest, MessageResponse>
{
    private readonly ILogger<LoginMessage> _logger = logger;
    private readonly IUsersServiceImplementation _implementation = implementation;

    public Task<MessageResponse> Handle(LoginRequest request, CancellationToken token)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            _logger.LogError("Email or Password can not be empty");
            throw new BadRequestException("Email or Password can not be empty");
        }

        try
        {
            var email = request.Email;
            var password = request.Password;

            return Task.FromResult(MessageResponse.Ok(_implementation.AuthUser(email, password)));
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