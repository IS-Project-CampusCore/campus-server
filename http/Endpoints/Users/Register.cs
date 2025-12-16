using commons.Protos;
using FastEndpoints;
using http.Endpoints;
using usersServiceClient;

namespace http.Enpoints.Users;

public record RegisterApiRequest(string Email, string Name, string Role);

public class Register(ILogger<Register> logger) : CampusEndpoint<RegisterApiRequest>(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/users/register");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterApiRequest req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.Name) || string.IsNullOrEmpty(req.Role))
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        var grpcRequest = new RegisterRequest
        {
            Email = req.Email,
            Name = req.Name,
            Role = req.Role,
        };

        MessageResponse grpcResponse = Client.Register(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}