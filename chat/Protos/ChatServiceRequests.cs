using commons.Protos;
using MediatR;

namespace chatServiceClient;

public partial class CreateGroupRequest : IRequest<MessageResponse>;
public partial class AddMemberRequest : IRequest<MessageResponse>;
public partial class RemoveMemberRequest : IRequest<MessageResponse>;
public partial class GetGroupsRequest : IRequest<MessageResponse>;

public partial class UploadFileRequest : IRequest<MessageResponse>;
public partial class GetFileRequest : IRequest<MessageResponse>;

public partial class SendMessageRequest : IRequest<MessageResponse>;
public partial class GetHistoryRequest : IRequest<MessageResponse>;

