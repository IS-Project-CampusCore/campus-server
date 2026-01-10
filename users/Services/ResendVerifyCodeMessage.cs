using commons.Protos;
using commons.RequestBase;
using users.Implementation;
using usersServiceClient;

namespace users.Services;

public class ResendVerifyCodeMessage(
    ILogger<ResendVerifyCodeMessage> logger,
    IUsersServiceImplementation implementation
) : CampusMessage<ResendVerifyCodeRequest, MessageResponse>(logger)
{
    private readonly IUsersServiceImplementation _impl = implementation;

    protected override async Task<MessageResponse> HandleMessage(ResendVerifyCodeRequest request, CancellationToken token)
    {
        await _impl.ResendVerifyCode(request.Email);

        return MessageResponse.Ok("Verification code has been sent.");
    }
}