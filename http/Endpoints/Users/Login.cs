using commons.Protos;
using FastEndpoints;
using usersServiceClient;

namespace http.Enpoints.Users;

public record LoginApiRequest(string Email, string Password);

public class Login : Endpoint<LoginApiRequest, MessageResponse>
{
    public usersService.usersServiceClient userServiceClient { get; set; } = default!;

    public override void Configure()
    {
        Post("api/users/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginApiRequest req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.Password))
        {
            await Send.ErrorsAsync(400, cancellationToken);
            return;
        }

        var grpcRequest = new LoginRequest
        {
            Email = req.Email,
            Password = req.Password,
        };

        try
        {
            MessageResponse grpcResponse = userServiceClient.Login(grpcRequest, null, null, cancellationToken);

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