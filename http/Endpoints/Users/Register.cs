using commons.Protos;
using FastEndpoints;
using usersServiceClient;

namespace http.Enpoints.Users;

public record RegisterApiRequest(string Email, string Name, string Role);

public class Register : Endpoint<RegisterApiRequest, MessageResponse>
{
    public usersService.usersServiceClient userServiceClient { get; set; } = default!;

    public override void Configure()
    {
        Post("api/users/register");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterApiRequest req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.Name) || string.IsNullOrEmpty(req.Role))
        {
            await Send.ErrorsAsync(400, cancellationToken);
            return;
        }

        var grpcRequest = new RegisterRequest
        {
            Email = req.Email,
            Name = req.Name,
            Role = req.Role,
        };

        try
        {
            MessageResponse grpcResponse = userServiceClient.Register(grpcRequest, null, null, cancellationToken);

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