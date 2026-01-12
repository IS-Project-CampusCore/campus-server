using commons.Protos;
using commons.RequestBase;
using MediatR;
using Microsoft.AspNetCore.Components.Web;
using MongoDB.Driver.Core.Servers;
using Serilog.Sinks.File;
using System.Xml.Linq;
using static MongoDB.Bson.Serialization.Serializers.SerializerHelper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace chatServiceClient;

public partial class CreateGroupRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(AdminId))
            return "Create Group request is empty";
        return null;
    }
}

public partial class DeleteGroupRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(GroupId) || string.IsNullOrEmpty(AdminId))
            return "Delete Group request is empty";
        return null;
    }
}

public partial class AddMemberRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(GroupId) || string.IsNullOrEmpty(MemberId))
            return "Add Group Member request is empty";
        return null;
    }
}
public partial class RemoveMemberRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(GroupId) || string.IsNullOrEmpty(MemberId))
            return "Remove Group Member request is empty";
        return null;
    }
}

public partial class LeaveGroupRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(GroupId) || string.IsNullOrEmpty(MemberId))
            return "Leave Group request is empty";
        return null;
    }
}

public partial class GetUserGroupsRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(MemberId) ? "Get User Groups request is empty" : null;
}

public partial class GetGroupRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(GroupId) ? "Get Group request is empty" : null;
}

public partial class GetGroupMembersRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(GroupId) ? "Get Group Members request is empty" : null;
}

public partial class UploadFileRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(GroupId) || string.IsNullOrEmpty(Name) || Data.IsEmpty)
            return "Upload File request is empty";
        return null;
    }
}
public partial class GetFileRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(FileId) ? "Get File request is empty" : null;
}

public partial class GetMessageFilesRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(MessageId) ? "Get Message Files request is empty" : null;
}

public partial class SendMessageRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(SenderId) || string.IsNullOrEmpty(GroupId))
            return "Missing Sender and Reciver ID";
        if (!HasContent && (FilesId is null || FilesId.Count == 0))
            return "Request contains an empty message";
        return null;
    }
}
public partial class GetHistoryRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(ReciverId) || string.IsNullOrEmpty(GroupId))
            return "Missing Reciver and Group ID";
        if (Skip < 0 || Limit < 1 || Limit > 20)
            return "Skip or Limit invalid";
        return null;
    }
}

