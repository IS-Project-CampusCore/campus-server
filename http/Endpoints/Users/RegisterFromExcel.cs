using commons.Protos;
using FastEndpoints;
using http.Auth;
using http.Endpoints;
using usersServiceClient;

namespace http.Enpoints.Users;

public record FileRequest(string FileName);

public class RegisterFromExcel(ILogger<RegisterFromExcel> logger) : CampusEndpoint<FileRequest>(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/users/register-excel");

        Policies(CampusPolicy.AuthenticatedUser);
        Roles("management");
    }

    public override async Task HandleAsync(FileRequest req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrEmpty(req.FileName))
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        var grpcRequest = new UsersExcelRequest
        {
            FileName = req.FileName
        };

        MessageResponse grpcResponse = await Client.RegisterUsersFromExcelAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}