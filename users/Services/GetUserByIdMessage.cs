using commons.Protos;
using commons.RequestBase;
using usersServiceClient;
using users.Model;

namespace users.Services;

public class GetUserByIdMessage(
    ILogger<GetUserByIdMessage> logger,
    IUsersServiceImplementation implementation
) : CampusMessage<UserIdRequest, User?>(logger)
{
    private readonly IUsersServiceImplementation _implementation = implementation;

    protected override Task<User?> HandleMessage(UserIdRequest request, CancellationToken token) =>
        _implementation.GetUserById(request.Id);
}
