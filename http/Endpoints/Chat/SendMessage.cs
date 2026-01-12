using chatServiceClient;
using commons.Protos;
using Google.Protobuf;
using http.Auth;

namespace http.Endpoints.Chat;

public record SendMessageApiRequest(string GroupId, string? Content, IEnumerable<IFormFile>? Files);

public class SendMessage(ILogger<SendMessage> logger) : CampusEndpoint<SendMessageApiRequest>(logger)
{
    public chatService.chatServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/chat/send");
        AllowFileUploads();

        Policies(CampusPolicy.AuthenticatedUser);
        Roles("student", "professor", "campus_student");
    }

    public override async Task HandleAsync(SendMessageApiRequest req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrEmpty(req.GroupId))
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        if (req.Content is null && (req.Files is null || !req.Files.Any()))
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        var grpcRequest = new SendMessageRequest
        {
            SenderId = GetUserId(),
            GroupId = req.GroupId,
            Content = req.Content
        };

        if (req.Files is not null && req.Files.Any()) 
        {
            foreach (var file in req.Files)
            {
                string fileName = file.FileName;

                using MemoryStream ms = new();
                await file.CopyToAsync(ms, cancellationToken);

                byte[] bytes = ms.ToArray();

                var uploadRequest = new UploadFileRequest
                {
                    Name = fileName,
                    GroupId = req.GroupId,
                    Data = ByteString.CopyFrom(bytes)
                };

                MessageResponse uploadResponse = await Client.UploadFileAsync(uploadRequest);
                if (!uploadResponse.Success)
                {
                    logger.LogError($"Upload file failed at File:{fileName}, Error:{uploadResponse.Errors}");

                    await SendAsync(uploadResponse, cancellationToken);
                    return;
                }

                var payload = uploadResponse.Payload;
                string fileId = payload.GetString("Id");

                grpcRequest.FilesId.Add(fileId);
            }
        }

        MessageResponse grpcResponse = await Client.SendMessageAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}