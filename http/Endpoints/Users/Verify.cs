using commons.Protos;
using FastEndpoints;
using usersServiceClient;

namespace http.Enpoints.Users;

public record VerifyApiRequest(string Email, string Password, string Code);

public class Verify : Endpoint<VerifyApiRequest, MessageResponse>
{
    public usersService.usersServiceClient userServiceClient { get; set; } = default!;

    public override void Configure()
    {
        Post("api/users/verify");
        AllowAnonymous();
    }

    public override async Task HandleAsync(VerifyApiRequest req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.Password) || string.IsNullOrEmpty(req.Code))
        {
            await Send.ErrorsAsync(400, cancellationToken);
            return;
        }

        var grpcRequest = new VerifyRequest
        {
            Email = req.Email,
            Password = req.Password,
            Code = req.Code,
        };

        try
        {
            MessageResponse grpcResponse = userServiceClient.Verify(grpcRequest, null, null, cancellationToken);

            if (grpcResponse.Success)
            {
                await Send.OkAsync(grpcResponse);
                return;
            }

            await Send.ErrorsAsync(grpcResponse.Code);
        }
        catch (Exception)
        {
            await Send.ErrorsAsync(500);
        }
    }
}