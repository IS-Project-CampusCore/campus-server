using commons.RequestBase;
using users.Implementation;
using users.Model;
using usersServiceClient;

namespace users.Services;

public class GetUserByEmailMessage(
    ILogger<GetUserByEmailMessage> logger,
    IUsersServiceImplementation implementation
) : CampusMessage<UserEmailRequest, User?>(logger)
{
    private readonly IUsersServiceImplementation _implementation = implementation;

    protected override async Task<User?> HandleMessage(UserEmailRequest request, CancellationToken token) =>
        await _implementation.GetUserByEmail(request.Email);
}
