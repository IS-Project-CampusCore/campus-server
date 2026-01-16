using commons.RequestBase;
using users.Implementation;
using users.Model;
using usersServiceClient;

namespace users.Services;

public class GetUsersByRoleMessage(
    ILogger<GetUsersByRoleMessage> logger,
    IUsersServiceImplementation implementation
) : CampusMessage<UsersRoleRequest, List<User>?>(logger)
{
    private readonly IUsersServiceImplementation _implementation = implementation;

    protected override async Task<List<User>?> HandleMessage(UsersRoleRequest request, CancellationToken token) =>
        await _implementation.GetUsersByRole(User.StringToRole(request.Role));
}
