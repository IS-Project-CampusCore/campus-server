using chatServiceClient;
using commons.Protos;
using http.Auth;
using Microsoft.AspNetCore.StaticFiles;
using System.IO.Compression;

namespace http.Endpoints.Chat;

public class GetFiles(ILogger<GetFiles> logger) : CampusEndpoint<string>(logger)
{
    public chatService.chatServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/chat/files");
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

        var grpcRequest = new GetMessageFilesRequest
        {
            MessageId = req
        };

        MessageResponse grpcResponse = await Client.GetMessageFilesAsync(grpcRequest, null, null, cancellationToken);
        if (grpcResponse is null || string.IsNullOrEmpty(grpcResponse.Body))
        {
            await HandleErrorsAsync(grpcResponse?.Code ?? 500, grpcResponse?.Errors ?? "Internal Error", cancellationToken);
            return;
        }

        var payload = grpcResponse.Payload;

        var files = payload.Array().Iterate();

        using var zipStream = new MemoryStream();

        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var file in files)
            {
                string fileName = file.GetString("FileName");
                byte[] data = file.GetBytesFromBase64("Data");

                var zipEntry = archive.CreateEntry(fileName, CompressionLevel.Fastest);

                using var entryStream = zipEntry.Open();
                await entryStream.WriteAsync(data, 0, data.Length, cancellationToken);
            }
        }

        zipStream.Position = 0;

        await Send.StreamAsync(
            stream: zipStream,
            fileName: $"attachments_{req}.zip",
            fileLengthBytes: zipStream.Length,
            contentType: "application/zip",
            cancellation: cancellationToken
        );
    }
}
