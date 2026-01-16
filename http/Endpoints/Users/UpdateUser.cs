using commons.Protos;
using http.Auth;
using http.Enpoints.Users;
using usersServiceClient;

namespace http.Endpoints.Users;

public record UpdateApiRequest(string Email, string? Name, string? Role, string? University, int? Year, int? Group, string? Major, string? Dormitory, string? Room, string? Department, string? Title);
public class UpdateUser(ILogger<UpdateUser> logger) : CampusEndpoint<UpdateApiRequest>(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/users/update");
        Policies(CampusPolicy.AuthenticatedUser);

        Roles("management");
    }

    public override async Task HandleAsync(UpdateApiRequest req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrEmpty(req.Email))
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        var grpcRequest = new UpdateUserRequest
        {
            Email = req.Email
        };

        if (!string.IsNullOrEmpty(req.Name))
            grpcRequest.Name = req.Name;
        if (!string.IsNullOrEmpty(req.Role))
            grpcRequest.Role = req.Role;
        if (!string.IsNullOrEmpty(req.University))
            grpcRequest.University = req.University;
        if (req.Year.HasValue)
            grpcRequest.Year = req.Year.Value;
        if (req.Group.HasValue)
            grpcRequest.Group = req.Group.Value;
        if (!string.IsNullOrEmpty(req.Major))
            grpcRequest.Major = req.Major;
        if (!string.IsNullOrEmpty(req.Dormitory))
            grpcRequest.Dormitory = req.Dormitory;
        if (!string.IsNullOrEmpty(req.Room))
            grpcRequest.Room = req.Room;
        if (!string.IsNullOrEmpty(req.Department))
            grpcRequest.Department = req.Department;
        if (!string.IsNullOrEmpty(req.Title))
            grpcRequest.Title = req.Title;

        MessageResponse grpcResponse = await Client.UpdateUserAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}
