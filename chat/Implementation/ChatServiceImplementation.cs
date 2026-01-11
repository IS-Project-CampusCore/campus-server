using chat.Models;
using commons.Database;
using commons.EventBase;
using commons.RequestBase;
using commons.Tools;
using MongoDB.Driver;
using System.Xml.Linq;
using usersServiceClient;

namespace Chat.Implementation;

public interface IChatService
{
    Task<Group> CreateGroupAsync(string name, string adminId);
    Task DeleteGroupAsync(string groupId, string adminId);
    Task DeleteGroupCleanupAsync(string groupId);
    Task AddGroupMemberAsync(string groupId, string memberId);
    Task RemoveGroupMemberAsync(string groupId, string memberId);
    Task LeaveGroupAsync(string groupId, string memberId);
    Task<IEnumerable<Group>?> GetUserGroupsAsync(string memberId);
    Task<Group> GetGroupByIdAsync(string groupId);

    Task<ChatFile> UploadFileAsync(string name, string groupId, byte[] data);
    Task<byte[]> GetFileByIdAsync(string fileId);
    Task<IEnumerable<byte[]>?> GetFilesByMessageIdAsync(string messageId);

    Task<ChatMessage> SendMessageAsync(string senderId, string groupId, string? content, List<string> filesId);
    Task<IEnumerable<ChatMessage>> GetMessagesAsync(string reciverId, string groupId, int skip, int limit);
}

public class ChatServiceImplementation(
    ILogger<ChatServiceImplementation> logger,
    IDatabase database,
    IScopedMessagePublisher publisher,
    usersService.usersServiceClient usersService,
    IConfiguration config
) : IChatService
{
    private readonly ILogger<ChatServiceImplementation> _logger = logger;

    private readonly AsyncLazy<IDatabaseCollection<ChatMessage>> _messages = new(() => GetMessagesCollection(database));
    private readonly AsyncLazy<IDatabaseCollection<Group>> _groups = new(() => GetGroupsCollection(database));
    private readonly AsyncLazy<IDatabaseCollection<ChatFile>> _files = new(() => GetFilesCollection(database));

    private readonly IScopedMessagePublisher _publisher = publisher;

    private readonly usersService.usersServiceClient _usersService = usersService;

    private string _basePath => config["StorageDir"] ?? "FileStorage/ChatFiles";

    public async Task<Group> CreateGroupAsync(string name, string adminId)
    {
        if (string.IsNullOrEmpty(name))
        {
            _logger.LogInformation("Group name can not be empty or null");
            throw new BadRequestException("Group Name can not be empty");
        }

        string adminName = await GetUserName(adminId);
        _logger.LogInformation($"New Group:{name} has been created with Admin:{adminName} at Time:{DateTime.UtcNow}");

        Group newGroup = new Group
        {
            Name = name,
            AdminId = adminId,
            MembersId = [adminId],
            CreatedAt = DateTime.UtcNow
        };

        var groups = await _groups;

        string groupId = await groups.InsertAsync(newGroup);

        await _publisher.Publish("GroupCreated", new
        {
            GroupId = groupId,
            GroupName = name,
            AdminId = adminId
        });

        return newGroup;
    }

    public async Task DeleteGroupAsync(string groupId, string adminId)
    {
        var group = await GetGroupByIdAsync(groupId);
        if (group is null)
        {
            _logger.LogInformation($"Group:{groupId} not found");
            throw new NotFoundException("Group not found.");
        }

        if (adminId != group.AdminId)
        {
            _logger.LogInformation($"Group can be deleted just by the Admin");
            throw new BadRequestException("Non admin members cannot delete groups.");
        }

        var groups = await _groups;
        await groups.DeleteWithIdAsync(groupId);

        await _publisher.Publish("GroupDeleted", new
        {
            GroupId = groupId,
            GroupName = group.Name,
            GroupMemberIds = group.MembersId
        });
    }

    public async Task DeleteGroupCleanupAsync(string groupId)
    {
        if (!await DeleteGroupFilesAsync(groupId))
        {
            _logger.LogError("Delete files from database failed");
            throw new InternalErrorException("Fail to delete from database");
        }

        if (!await DeleteGroupMessagesAsync(groupId))
        {
            _logger.LogError("Delete files from database failed");
            throw new InternalErrorException("Fail to delete from database");
        }
    }

    public async Task<Group> GetGroupByIdAsync(string groupId)
    {
        var groups = await _groups;

        return await groups.GetOneByIdAsync(groupId);
    }

    public async Task AddGroupMemberAsync(string groupId, string memberId)
    {
        Group group = await GetGroupByIdAsync(groupId);
        if (group is null)
        {
            _logger.LogInformation($"Group not found for Id:{groupId}");
            throw new BadRequestException("Group not found");
        }

        if (group.MembersId.Contains(memberId))
        {
            _logger.LogInformation($"Member:{memberId} is already added to this Group:{group.Name}");
            throw new BadRequestException("User already added");
        }

        string memberName = await GetUserName(memberId);

        group.MembersId.Add(memberId);

        var groups = await _groups;
        await groups.ReplaceAsync(group);

        _logger.LogInformation($"New Memeber:{memberName} was added to Group:{group.Name}");

        await _publisher.Publish("MemberAdded", new
        {
            GroupId = groupId,
            MemberId = memberId,
            MemberName = memberName
        });
    }

    public async Task RemoveGroupMemberAsync(string groupId, string memberId)
    {
        var group = await GetGroupByIdAsync(groupId);
        if (group is null)
        {
            _logger.LogInformation($"Group not found for Id:{groupId}");
            throw new NotFoundException("Group not found");
        }

        if (!group.MembersId.Contains(memberId))
        {
            _logger.LogInformation($"Member:{memberId} is not a member in Group:{groupId}");
            throw new NotFoundException("User not found in group");
        }

        if (group.AdminId == memberId)
        {
            _logger.LogInformation("Admin can not be removed by members");
            throw new BadRequestException("Cannot remove the group Admin");
        }

        await RemoveMember(group, memberId, false);
    }

    public async Task LeaveGroupAsync(string groupId, string memberId)
    {
        var group = await GetGroupByIdAsync(groupId);
        if (group is null)
        {
            _logger.LogInformation($"Group not found for Id:{groupId}");
            throw new NotFoundException("Group not found");
        }

        if (!group.MembersId.Contains(memberId))
            return;

        if (group.AdminId == memberId)
        {
            if (group.MembersId.Count == 1)
            {
                _logger.LogInformation($"Last member leaved the Group:{group.Name}, continue with Delete Group");
                await DeleteGroupAsync(groupId, group.AdminId);
                return;
            }

            string newAdminId = group.MembersId.First(m => m != memberId);
            group.AdminId = newAdminId;
            _logger.LogInformation($"Admin changed to {newAdminId}");
        }

        await RemoveMember(group, memberId, true);
    }

    public async Task<IEnumerable<Group>?> GetUserGroupsAsync(string memberId)
    {
        var memberName = GetUserName(memberId);

        var groups = await _groups;
        var memberGroups = await groups.MongoCollection.FindAsync(gr => gr.MembersId.Contains(memberId));

        var res = await memberGroups.ToListAsync();

        _logger.LogInformation($"Number of Groups found for user:{memberName} is {res.Count}");
        return res;
    }

    public async Task<ChatFile> UploadFileAsync(string name, string groupId, byte[] data)
    {
        Group group = await GetGroupByIdAsync(groupId);
        if (group is null)
        {
            _logger.LogInformation($"Group not found for Id:{groupId}");
            throw new BadRequestException("Group not found");
        }

        string extension = Path.GetExtension(name);
        string nameWhitoutExtension = Path.GetFileNameWithoutExtension(name);
        string fileName = $"{nameWhitoutExtension}_{Guid.NewGuid().ToString()}{extension}";

        var filePath = Path.Combine(_basePath, group.Id, fileName);
        var directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            _logger.LogInformation($"New directory was created for Group:{group.Name}");
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllBytesAsync(filePath, data);

        ChatFile file = new ChatFile
        {
            FileName = fileName,
            GroupId = groupId,
            FilePath = filePath,
            UploadedAt = DateTime.UtcNow
        };

        var files = await _files;

        _logger.LogInformation($"Upload file with Name:{file.FileName}");
        await files.InsertAsync(file);

        return file;
    }

    public async Task<byte[]> GetFileByIdAsync(string fileId)
    {
        var files = await _files;

        ChatFile file = await files.GetOneByIdAsync(fileId);
        if (file is null)
        {
            _logger.LogInformation($"No file was found in database with Id:{fileId}");
            throw new NotFoundException($"File with Id:{fileId} not found");
        }

        try
        {
            using var fileStream = File.OpenRead(file.FilePath);
            using MemoryStream ms = new();
            await fileStream.CopyToAsync(ms);

            return ms.ToArray();
        }
        catch (FileNotFoundException)
        {
            _logger.LogInformation($"No file was found in storage with Id:{fileId}");

            if (await files.DeleteWithIdAsync(fileId))
            {
                _logger.LogInformation($"File:{fileId} deleted");
            }

            throw new NotFoundException($"File with Id:{fileId} not found");
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex.Message);
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<IEnumerable<byte[]>?> GetFilesByMessageIdAsync(string messageId)
    {
        var messages = await _messages;

        ChatMessage message = await messages.GetOneByIdAsync(messageId);
        if (message is null)
        {
            _logger.LogInformation($"No message was found in database with Id:{messageId}");
            throw new NotFoundException($"Message with Id:{messageId} not found");
        }

        if (message.FilesId is null || !message.FilesId.Any())
        {
            _logger.LogInformation($"Message:{messageId} contains no files. Null will be sent.");
            return null;
        }

        List<byte[]> files = [];
        foreach (var fileId in message.FilesId)
        {
            var file = await GetFileByIdAsync(fileId);
            files.Add(file);
        }

        return files;
    }

    public async Task<ChatMessage> SendMessageAsync(string senderId, string groupId, string? content, List<string> filesId)
    {
        if (content is null && (filesId is null || filesId.Count <= 0))
        {
            _logger.LogInformation("Message can not be empty");
            throw new BadRequestException("Message can not be empty");
        }

        var senderName = GetUserName(senderId); 

        var group = await GetGroupByIdAsync(groupId);
        if (group is null)
        {
            _logger.LogInformation($"Group with Id:{groupId} was not found");
            throw new NotFoundException("Group not found");
        }

        var message = new ChatMessage
        {
            SenderId = senderId,
            GroupId = groupId,
            Content = content,
            FilesId = filesId,
            SentAt = DateTime.UtcNow,
        };

        var messages = await _messages;
        string id = await messages.InsertAsync(message);

        await _publisher.Publish("MessageCreated", message);

        return message;
    }

    public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(string reciverId, string groupId,int skip, int limit)
    {
        if (skip < 0)
        {
            _logger.LogInformation("Skip can not be less than 0");
            throw new BadRequestException("Invalid skip number");
        }

        if (limit < 1 || limit > 20)
        {
            _logger.LogInformation("Limit needs to be between 1 and 20");
            throw new BadRequestException("Invalid message number");
        }

        var reciverName = GetUserName(reciverId);

        var group = await GetGroupByIdAsync(groupId);
        if (group is null)
        {
            _logger.LogInformation($"Group with Id:{groupId} was not found");
            throw new NotFoundException("Group not found");
        }

        if (group.MembersId is null || !group.MembersId.Contains(reciverId))
        {
            _logger.LogInformation($"User:{reciverName} is not a member of Group:{group.Name}");
            throw new BadRequestException("User is not member of group");
        }

        var messages = await _messages;
        var chats = messages.MongoCollection.Aggregate()
            .Match(cm => cm.GroupId == groupId)
            .SortByDescending(cm => cm.SentAt)
            .Skip(skip)
            .Limit(limit);

        return await chats.ToListAsync();
    }

    private async Task RemoveMember(Group group, string memberId, bool isLeave)
    {
        string? memberName = null;
        try
        {
            memberName = await GetUserName(memberId);
        }
        catch (Exception ex) 
        {
            _logger.LogInformation($"Get User Name failed with Exception:{ex.GetType().Name}, Msg:{ex.Message}, continue with name Unknown");
            memberName = "Unknown";
        }

        group.MembersId.Remove(memberId);

        var groups = await _groups;
        await groups.ReplaceAsync(group);

        string eventName = isLeave ? "MemberLeft" : "MemberRemoved";

        _logger.LogInformation($"User {memberId} processed: {eventName}");
        await _publisher.Publish(eventName, new
        {
            GroupId = group.Id,
            MemberId = memberId,
            MemberName = memberName,
            NewAdminId = group.AdminId
        });
    }

    private async Task<bool> DeleteGroupMessagesAsync(string groupId)
    {
        var messages = await _messages;
        var groupFilter = Builders<ChatMessage>.Filter.Eq(cm => cm.GroupId, groupId);

        var res = await messages.MongoCollection.DeleteManyAsync(groupFilter);
        return res.IsAcknowledged;
    }

    private async Task<bool> DeleteGroupFilesAsync(string groupId)
    {
        var directoryPath = Path.Combine(_basePath, groupId);
        if (Directory.Exists(directoryPath))
        {
            try
            {
                Directory.Delete(directoryPath, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete folder for Group:{groupId}, Exception:{ex.GetType().Name}, Msg:{ex.Message}");
                throw new InternalErrorException("Group directory could not be deleted");
            }
        }

        var files = await _files;
        var groupFilter = Builders<ChatFile>.Filter.Eq(cf => cf.GroupId, groupId);

        var res = await files.MongoCollection.DeleteManyAsync(groupFilter);
        return res.IsAcknowledged;
    }

    private async Task<string> GetUserName(string userId)
    {
        var getMemberRes = await _usersService.GetUserByIdAsync(new UserIdRequest { Id = userId });

        if (!getMemberRes.Success)
        {
            _logger.LogInformation($"Get Member has failed with Code:{getMemberRes.Code} and Error:{getMemberRes.Errors}");
            throw new NotFoundException("Member not found.");
        }

        var payload = getMemberRes.Payload;
        return payload.GetString("Name");
    }

    internal static async Task<IDatabaseCollection<ChatMessage>> GetMessagesCollection(IDatabase database)
    {
        var collection = database.GetCollection<ChatMessage>();

        var senderIndex = Builders<ChatMessage>.IndexKeys.Ascending(cm => cm.SenderId);
        var groupIndex = Builders<ChatMessage>.IndexKeys.Ascending(cm => cm.GroupId);
        var sendAtIndex = Builders<ChatMessage>.IndexKeys.Descending(cm => cm.SentAt);

        CreateIndexModel<ChatMessage> index1 = new(senderIndex, new CreateIndexOptions
        {
            Name = "messageSenderIndex"
        });

        CreateIndexModel<ChatMessage> index2 = new(groupIndex, new CreateIndexOptions
        {
            Name = "messageGroupIndex"
        });

        CreateIndexModel<ChatMessage> index3 = new(sendAtIndex, new CreateIndexOptions
        {
            Name = "messageSendAtIndex"
        });

        await collection.MongoCollection.Indexes.CreateManyAsync([index1, index2, index3]);
        return collection;
    }

    internal static async Task<IDatabaseCollection<Group>> GetGroupsCollection(IDatabase database)
    {
        var collection = database.GetCollection<Group>();

        var nameIndex = Builders<Group>.IndexKeys.Ascending(gr => gr.Name);

        CreateIndexModel<Group> index1 = new(nameIndex, new CreateIndexOptions
        {
            Name = "groupNameIndex"
        });

        await collection.MongoCollection.Indexes.CreateManyAsync([index1]);
        return collection;
    }

    internal static async Task<IDatabaseCollection<ChatFile>> GetFilesCollection(IDatabase database)
    {
        var collection = database.GetCollection<ChatFile>();

        var nameIdex = Builders<ChatFile>.IndexKeys.Ascending(cf => cf.FileName);
        var senderIndex = Builders<ChatFile>.IndexKeys.Ascending(cf => cf.GroupId);

        CreateIndexModel<ChatFile> index1 = new(nameIdex, new CreateIndexOptions
        {
            Name = "fileNameIndex"
        });

        CreateIndexModel<ChatFile> index2 = new(senderIndex, new CreateIndexOptions
        {
            Name = "fileSenderIndex"
        });

        await collection.MongoCollection.Indexes.CreateManyAsync([index1, index2]);
        return collection;
    }
}