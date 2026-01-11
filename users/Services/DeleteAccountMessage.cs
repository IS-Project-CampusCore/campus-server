using commons.Protos;
using commons.RequestBase;
using users.Implementation;
using usersServiceClient;

namespace users.Services;

public class DeleteAccountMessage(
    ILogger<DeleteAccountMessage> logger,
    IUsersServiceImplementation implementation
) : CampusMessage<DeleteAccountRequest>(logger)
{
    private readonly IUsersServiceImplementation _impl = implementation;

    protected override async Task HandleMessage(DeleteAccountRequest request, CancellationToken token)
    {
        await _impl.DeleteAccount(request.UserId);
    }
}