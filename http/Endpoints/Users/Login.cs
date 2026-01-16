using commons.Protos;
using http.Endpoints;
using usersServiceClient;

namespace http.Enpoints.Users;

public record LoginApiRequest(string Email, string Password);

public class Login(ILogger<Login> logger) : CampusEndpoint<LoginApiRequest>(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/users/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginApiRequest req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.Password))
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        var grpcRequest = new LoginRequest
        {
            Email = req.Email,
            Password = req.Password,
        };

        MessageResponse grpcResponse = await Client.LoginAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}