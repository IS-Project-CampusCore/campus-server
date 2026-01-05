using commons.Protos;
using commons.RequestBase;
using users.Model;
using usersServiceClient;

namespace users.Services;

public class LoginMessage(
    ILogger<LoginMessage> logger,
    IUsersServiceImplementation implementation
) : CampusMessage<LoginRequest, UserWithJwt>(logger)
{
    private readonly IUsersServiceImplementation _implementation = implementation;

    protected override Task<UserWithJwt> HandleMessage(LoginRequest request, CancellationToken token) =>
        Task.FromResult(_implementation.AuthUser(request.Email, request.Password));
}