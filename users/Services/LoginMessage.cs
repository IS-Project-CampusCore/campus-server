using commons.Protos;
using commons.RequestBase;
using users.Implementation;
using users.Model;
using usersServiceClient;

namespace users.Services;

public class LoginMessage(
    ILogger<LoginMessage> logger,
    IUsersServiceImplementation implementation
) : CampusMessage<LoginRequest, UserWithJwt>(logger)
{
    private readonly IUsersServiceImplementation _implementation = implementation;

    protected override async Task<UserWithJwt> HandleMessage(LoginRequest request, CancellationToken token) =>
        await _implementation.AuthUser(request.Email, request.Password);
}