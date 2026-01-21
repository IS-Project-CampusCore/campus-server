using chatServiceClient;
using commons.Protos;
using http.Auth;
using Microsoft.AspNetCore.StaticFiles;

namespace http.Endpoints.Chat;

public class GetFile(ILogger<GetFile> logger) : CampusEndpoint<string>(logger)
{
    public chatService.chatServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/chat/file");
        Policies(CampusPolicy.AuthenticatedUser);

        Roles("student", "professor", "campus_student");
    }

    public override async Task HandleAsync(string req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req))
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        var grpcRequest = new GetFileRequest
        {
            FileId = req
        };

        MessageResponse grpcResponse = await Client.GetFileAsync(grpcRequest, null, null, cancellationToken);
        if (grpcResponse is null || string.IsNullOrEmpty(grpcResponse.Body))
        {
            await HandleErrorsAsync(grpcResponse?.Code ?? 500, grpcResponse?.Errors ?? "Internal Error", cancellationToken);
            return;
        }

        var payload = grpcResponse.Payload;
        string fileName = payload.GetString("FileName");
        byte[] data = payload.GetBytesFromBase64("Data");

        var provider = new FileExtensionContentTypeProvider();

        if (!provider.TryGetContentType(fileName, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        await Send.BytesAsync(data, fileName, contentType, null, false, cancellationToken);
    }
}
