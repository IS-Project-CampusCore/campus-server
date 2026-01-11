using commons.Protos;
using commons.RequestBase;
using Grpc.Core;
using MediatR;
using users.Implementation;
using users.Model;
using usersServiceClient;

namespace users.Services;

public class VerifyMessage(
    ILogger<LoginMessage> logger,
    IUsersServiceImplementation implementation
) : CampusMessage<VerifyRequest, UserWithJwt>(logger)
{
    private readonly IUsersServiceImplementation _implementation = implementation;

    protected override async Task<UserWithJwt> HandleMessage(VerifyRequest request, CancellationToken token)
    {
        var email = request.Email;
        var password = request.Password;
        var code = request.Code;

        return await _implementation.Verify(email, password, code);
    }
}