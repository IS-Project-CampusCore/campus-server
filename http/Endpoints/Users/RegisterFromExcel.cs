using commons.Protos;
using FastEndpoints;
using usersServiceClient;

namespace http.Enpoints.Users;

public record FileRequest(string FileName);

public class RegisterFromExcel : Endpoint<FileRequest, MessageResponse>
{
    public usersService.usersServiceClient userServiceClient { get; set; } = default!;

    public override void Configure()
    {
        Post("api/users/register-excel");
        AllowAnonymous();
    }

    public override async Task HandleAsync(FileRequest req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrEmpty(req.FileName))
        {
            await Send.ErrorsAsync(400, cancellationToken);
            return;
        }

        var grpcRequest = new UsersExcelRequest
        {
            FileName = req.FileName
        };

        try
        {
            MessageResponse grpcResponse = await userServiceClient.RegisterUsersFromExcelAsync(grpcRequest, null, null, cancellationToken);

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