using commons.Protos;
using chatServiceClient;
using Grpc.Core;
using MediatR;

namespace Chat;

public class ChatService(IMediator mediator) : chatService.chatServiceBase
{
    private readonly IMediator _mediator = mediator;

    public override async Task<MessageResponse> CreateGroup(CreateGroupRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request, context.CancellationToken);
    }
    public override async Task<MessageResponse> DeleteGroup(DeleteGroupRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request, context.CancellationToken);
    }
    public override async Task<MessageResponse> AddGroupMember(AddMemberRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request, context.CancellationToken);
    }
    public override async Task<MessageResponse> RemoveGroupMember(RemoveMemberRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request, context.CancellationToken);
    }
    public override async Task<MessageResponse> LeaveGroup(LeaveGroupRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request, context.CancellationToken);
    }
    public override async Task<MessageResponse> GetUserGroups(GetUserGroupsRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request, context.CancellationToken);
    }
    public override async Task<MessageResponse> GetGroup(GetGroupRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request, context.CancellationToken);
    }
    public override async Task<MessageResponse> GetGroupMembers(GetGroupMembersRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request, context.CancellationToken);
    }

    public override async Task<MessageResponse> UploadFile(UploadFileRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request, context.CancellationToken);
    }
    public override async Task<MessageResponse> GetFile(GetFileRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request, context.CancellationToken);
    }
    public override async Task<MessageResponse> GetMessageFiles(GetMessageFilesRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request, context.CancellationToken);
    }

    public override async Task<MessageResponse> SendMessage(SendMessageRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request, context.CancellationToken);
    }
    public override async Task<MessageResponse> GetHistory(GetHistoryRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request, context.CancellationToken);
    }
}

