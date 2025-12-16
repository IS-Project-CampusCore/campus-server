using commons.Protos;
using FastEndpoints;
using http.Endpoints;
using usersServiceClient;

namespace http.Enpoints.Users;

public record VerifyApiRequest(string Email, string Password, string Code);

public class Verify(ILogger<Verify> logger) : CampusEndpoint<VerifyApiRequest>(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/users/verify");
        AllowAnonymous();
    }

    public override async Task HandleAsync(VerifyApiRequest req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.Password) || string.IsNullOrEmpty(req.Code))
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        var grpcRequest = new VerifyRequest
        {
            Email = req.Email,
            Password = req.Password,
            Code = req.Code,
        };

        MessageResponse grpcResponse = Client.Verify(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}