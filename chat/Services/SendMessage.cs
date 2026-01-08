using Grpc.Core;
using commons;
using commons.Protos;
using chatServiceClient;
using MediatR;
using Chat.Implementation;
using commons.RequestBase;
using chat.Models;

namespace Chat.Services;

public class SendMessage(
    ILogger<SendMessage> logger,
    ChatServiceImplementation implementation
) : CampusMessage<SendMessageRequest, ChatMessage>(logger)
{
    private readonly ChatServiceImplementation _impl = implementation;

    protected override async Task<ChatMessage> HandleMessage(SendMessageRequest request, CancellationToken token)
    {
        string sender = request.SenderId;
        string reciver = request.GroupId;
        string? content = request.Content;
        List<string> files = [.. request.FilesId];

        return await _impl.SendMessageAsync(sender, reciver, content, files);
    }
}
