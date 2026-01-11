using commons.Protos;
using commons.RequestBase;
using users.Implementation;
using users.Model;
using usersServiceClient;

namespace users.Services;

public class ResendVerifyCodeMessage(
    ILogger<ResendVerifyCodeMessage> logger,
    IUsersServiceImplementation implementation
) : CampusMessage<ResendVerifyCodeRequest>(logger)
{
    private readonly IUsersServiceImplementation _impl = implementation;

    protected override async Task HandleMessage(ResendVerifyCodeRequest request, CancellationToken token)
    {
        await _impl.ResendVerifyCode(request.Email);
    }
}