using commons.Protos;
using http.Endpoints;
using usersServiceClient;

namespace http.Enpoints.Users;

public record RegisterApiRequest(string Email, string Name, string Role, string? University, int? Year, int? Group, string? Major, string? Department, string? Title);

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
            Role = req.Role
        };

        if (!string.IsNullOrEmpty(req.University))
            grpcRequest.University = req.University;
        if (req.Year.HasValue)
            grpcRequest.Year = req.Year.Value;
        if (req.Group.HasValue)
            grpcRequest.Group = req.Group.Value;
        if (!string.IsNullOrEmpty(req.Major))
            grpcRequest.Major = req.Major;
        if (!string.IsNullOrEmpty(req.Department))
            grpcRequest.Department = req.Department;
        if (!string.IsNullOrEmpty(req.Title))
            grpcRequest.Title = req.Title;

        MessageResponse grpcResponse = await Client.RegisterAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}