using commons.Protos;
using commons.RequestBase;
using Grpc.Core;
using MediatR;
using users.Implementation;
using users.Model;
using usersServiceClient;

namespace users.Services;

public class RegisterMessage(
    ILogger<LoginMessage> logger,
    IUsersServiceImplementation implementation
) : CampusMessage<RegisterRequest, User?>(logger)
{
    private readonly IUsersServiceImplementation _implementation = implementation;

    protected override async Task<User?> HandleMessage(RegisterRequest request, CancellationToken token)
    {
        var email = request.Email;
        var name = request.Name;
        var role = User.StringToRole(request.Role);

        return await _implementation.RegisterUser(email, name, role);
    }
}