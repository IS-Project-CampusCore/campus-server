using commons.Protos;
using http.Auth;
using http.Endpoints.Users;
using usersServiceClient;

namespace http.Endpoints.Users;

public class UpdateFromExcel(ILogger<UpdateFromExcel> logger) : CampusEndpoint<FileRequest>(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/users/update-excel");

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

        var grpcRequest = new UpdateFromExcelRequest
        {
            FileName = req.FileName

        };

        MessageResponse grpcResponse = await Client.UpdateUsersFromExcelAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}
