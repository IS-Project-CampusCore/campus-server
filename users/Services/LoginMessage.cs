using Grpc.Core;
using usersServiceClient;
using commons;
using commons.Protos;

namespace users.Services;

public class LoginMessage : usersService.usersServiceBase
{
    private readonly ILogger<LoginMessage> _logger;

    public LoginMessage(ILogger<LoginMessage> logger)
    {
        _logger = logger;
    }

    public override Task<MessageResponse> Login(LoginRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            throw new BadRequestException("Email or password can not be empty");
        }

        return Task.FromResult(MessageResponse.Ok(responseBody));
    }
}