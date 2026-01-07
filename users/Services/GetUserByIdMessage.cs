using commons.Protos;
using commons.RequestBase;
using usersServiceClient;
using users.Model;
using users.Implementation;

namespace users.Services;

public class GetUserByIdMessage(
    ILogger<GetUserByIdMessage> logger,
    IUsersServiceImplementation implementation
) : CampusMessage<UserIdRequest, User?>(logger)
{
    private readonly IUsersServiceImplementation _implementation = implementation;

    protected override async Task<User?> HandleMessage(UserIdRequest request, CancellationToken token) =>
        await _implementation.GetUserById(request.Id);
}