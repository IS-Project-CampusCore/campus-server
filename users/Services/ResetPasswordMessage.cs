using commons.Protos;
using commons.RequestBase;
using users.Implementation;
using usersServiceClient;

namespace users.Services;

public class ResetPasswordMessage(
    ILogger<ResetPasswordMessage> logger,
    IUsersServiceImplementation implementation
) : CampusMessage<ResetPasswordRequest, MessageResponse>(logger)
{
    private readonly IUsersServiceImplementation _impl = implementation;

    protected override async Task<MessageResponse> HandleMessage(ResetPasswordRequest request, CancellationToken token)
    {
        await _impl.ResetPassword(request.Email);
        return MessageResponse.Ok("Reset password code sent to email.");
    }
}