using commons;
using commons.Protos;
using commons.RequestBase;
using System.Text.Json;
using users.Implementation;
using usersServiceClient;

namespace users.Services;

public class GetAllUsersMessage(
    ILogger<GetAllUsersMessage> logger,
    IUsersServiceImplementation implementation
) : CampusMessage<GetAllUsersRequest, List<Model.User>>(logger)
{
    private readonly IUsersServiceImplementation _impl = implementation;

    protected override async Task<List<Model.User>> HandleMessage(GetAllUsersRequest request, CancellationToken token)
    {
        return await _impl.GetAllUsers();

    }
}