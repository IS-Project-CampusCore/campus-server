using commons.Protos;
using commons.RequestBase;
using users.Implementation;
using usersServiceClient;

namespace users.Services;

public class ResetPasswordMessage(
    ILogger<ResetPasswordMessage> logger,
    IUsersServiceImplementation implementation
) : CampusMessage<ResetPasswordRequest>(logger)
{
    private readonly IUsersServiceImplementation _impl = implementation;

    protected override async Task HandleMessage(ResetPasswordRequest request, CancellationToken token)
    {
        await _impl.ResetPassword(request.Email);
    }
}