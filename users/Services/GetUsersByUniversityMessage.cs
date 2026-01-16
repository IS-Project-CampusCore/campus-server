using commons.RequestBase;
using users.Implementation;
using users.Model;
using usersServiceClient;

namespace users.Services;

public class GetUsersByUniversityMessage(
    ILogger<GetUsersByUniversityMessage> logger,
    IUsersServiceImplementation implementation
) : CampusMessage<UsersUniversityRequest, List<User>?>(logger)
{
    private readonly IUsersServiceImplementation _implementation = implementation;

    protected override async Task<List<User>?> HandleMessage(UsersUniversityRequest request, CancellationToken token) =>
        await _implementation.GetUsersByUniversity(request.University);
}
