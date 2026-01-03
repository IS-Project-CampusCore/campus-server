using chat.Models;
using commons;
using commons.Database;
using commons.Events;
using commons.Tools;
using MassTransit;
using MongoDB.Driver;
using MongoDB.Driver.Core.Servers;
using usersServiceClient;
using System.Linq;

namespace Chat.Implementation;

public interface IChatService
{
    public Task<string> CreateGroup(string name, string adminId);
    public Task AddGroupMember(string groupId, string memeberId);
    public Task RemoveGroupMember(string groupId, string memeberId);
    public Task<Group> GetGroupById(string groupId);
    public Task<IEnumerable<Group>> GetUserGroups(string memberId);

    public Task<string> UploadFile(string name, string groupId, byte[] data);
    public Task<FileStream> GetFileById(string fileId);

    public Task<string> SendMessage(string senderId, string groupId, string? content, List<string>? filesId);
    public Task<IEnumerable<ChatMessage>> GetMessages(string reciverId, string groupId, int skip, int limit);
}

public class ChatServiceImplementation(
    ILogger<ChatServiceImplementation> logger,
    IDatabase database,
    IPublishEndpoint publishEndpoint,
    usersService.usersServiceClient usersService,
    IConfiguration config
) : IChatService
{
    private readonly ILogger<ChatServiceImplementation> _logger = logger;

    private readonly AsyncLazy<IDatabaseCollection<ChatMessage>> _messages = new(() => GetMessagesCollection(database));
    private readonly AsyncLazy<IDatabaseCollection<Group>> _groups = new(() => GetGroupsCollection(database));
    private readonly AsyncLazy<IDatabaseCollection<ChatFile>> _files = new(() => GetFilesCollection(database));

    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    private readonly usersService.usersServiceClient _usersService = usersService;

    private string _basePath => config["StorageDir"] ?? "FileStorage/ChatFiles";

    public async Task<string> CreateGroup(string name, string adminId)
    {
        if (string.IsNullOrEmpty(name))
        {
            _logger.LogInformation("Group name can not be empty or null");
            throw new BadRequestException("Group Name can not be empty");
        }

        var getAdminRes = await _usersService.GetUserByIdAsync(new UserIdRequest { Id = adminId });

        if (!getAdminRes.Success)
        {
            _logger.LogInformation($"Get Admin has failed with Code:{getAdminRes.Code} and Error:{getAdminRes.Errors}");
            throw new BadRequestException("Group Admin Id is not valid");
        }

        var payload = getAdminRes.Payload;
        string adminName = getAdminRes.Payload.GetString("Name");

        _logger.LogInformation($"New Group:{name} has been created with Admin:{adminName} at Time:{DateTime.UtcNow}");

        Group newGroup = new Group
        {
            Name = name,
            AdminId = adminId,
            MembersId = [adminId],
            CreatedAt = DateTime.UtcNow
        };

        var groups = await _groups;

        return await groups.InsertAsync(newGroup);
    }

    public async Task<Group> GetGroupById(string groupId)
    {
        var groups = await _groups;

        return await groups.GetOneByIdAsync(groupId);
    }

    public async Task AddGroupMember(string groupId, string memeberId)
    {
        Group group = await GetGroupById(groupId);
        if (group is null)
        {
            _logger.LogInformation($"Group not found for Id:{groupId}");
            throw new BadRequestException("Group not found");
        }

        var getMemberRes = await _usersService.GetUserByIdAsync(new UserIdRequest { Id = memeberId });

        if (!getMemberRes.Success)
        {
            _logger.LogInformation($"Get Member has failed with Code:{getMemberRes.Code} and Error:{getMemberRes.Errors}");
            throw new NotFoundException("User not found");
        }

        var payload = getMemberRes.Payload;
        string memberName = payload.GetString("Name");

        group.MembersId.Add(memeberId);

        var groups = await _groups;
        await groups.ReplaceAsync(group);

        _logger.LogInformation($"New Memeber:{memberName} was added to Group:{group.Name}");
    }

    public async Task RemoveGroupMember(string groupId, string memeberId)
    {
        Group group = await GetGroupById(groupId);
        if (group is null)
        {
            _logger.LogInformation($"Group not found for Id:{groupId}");
            throw new BadRequestException("Group not found");
        }

        if (!group.MembersId.Contains(memeberId))
        {
            _logger.LogInformation($"No group memeber found with Id:{memeberId}");
        }

        var getMemberRes = await _usersService.GetUserByIdAsync(new UserIdRequest { Id = memeberId });

        if (!getMemberRes.Success)
        {
            _logger.LogInformation($"Get Member has failed with Code:{getMemberRes.Code} and Error:{getMemberRes.Errors}");
            throw new NotFoundException("User not found");
        }

        var payload = getMemberRes.Payload;
        string memberName = payload.GetString("Name");

        group.MembersId.Remove(memeberId);

        var groups = await _groups;
        await groups.ReplaceAsync(group);

        _logger.LogInformation($"Memeber:{memberName} was removed from Group:{group.Name}");
    }

    public async Task<IEnumerable<Group>> GetUserGroups(string memberId)
    {
        var memberRes = await _usersService.GetUserByIdAsync(new UserIdRequest { Id = memberId });
        if (!memberRes.Success)
        {
            _logger.LogInformation($"Invalid Sender Id");
            throw new BadRequestException(memberRes.Errors);
        }

        var payload = memberRes.Payload;
        string member = payload.GetString("Name");

        var groups = await _groups;
        var memberGroups = await groups.MongoCollection.FindAsync(gr => gr.MembersId.Contains(member));

        var res = await memberGroups.ToListAsync();

        _logger.LogInformation($"Number of Groups found for user:{member} is {res.Count}");
        return res;
    }

    public async Task<string> UploadFile(string name, string groupId, byte[] data)
    {
        Group group = await GetGroupById(groupId);
        if (group is null)
        {
            _logger.LogInformation($"Group not found for Id:{groupId}");
            throw new BadRequestException("Group not found");
        }

        string fileName = name + Guid.NewGuid().ToString();

        var filePath = Path.Combine(_basePath, group.Name, fileName);
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
        return await files.InsertAsync(file);
    }

    public async Task<FileStream> GetFileById(string fileId)
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
            return File.OpenRead(file.FilePath);
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

    public async Task<string> SendMessage(string senderId, string groupId, string? content, List<string>? filesId)
    {
        if (content is null && (filesId is null || filesId.Count <= 0))
        {
            _logger.LogInformation("Message can not be empty");
            throw new BadRequestException("Message can not be empty");
        }

        var senderRes = await _usersService.GetUserByIdAsync(new UserIdRequest { Id = senderId });
        if (!senderRes.Success)
        {
            _logger.LogInformation($"Invalid Sender Id");
            throw new BadRequestException(senderRes.Errors);
        }

        var payload = senderRes.Payload;
        string senderName = payload.GetString("Name");

        var group = await GetGroupById(groupId);
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
            SendAt = DateTime.UtcNow,
        };

        var messages = await _messages;
        string id = await messages.InsertAsync(message);

        await _publishEndpoint.Publish(new MessageCreatedEvent(
            senderId,
            groupId,
            content,
            filesId,
            message.SendAt
        ));

        return id;
    }

    public async Task<IEnumerable<ChatMessage>> GetMessages(string reciverId, string groupId,int skip, int limit)
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

        var reciverRes = await _usersService.GetUserByIdAsync(new UserIdRequest { Id = reciverId });
        if (!reciverRes.Success)
        {
            _logger.LogInformation($"Invalid Sender Id");
            throw new BadRequestException(reciverRes.Errors);
        }

        var payload = reciverRes.Payload;
        string reciverName = payload.GetString("Name");

        var group = await GetGroupById(groupId);
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
            .SortByDescending(cm => cm.SendAt)
            .Skip(skip)
            .Limit(limit);

        return await chats.ToListAsync();
    }

    internal static async Task<IDatabaseCollection<ChatMessage>> GetMessagesCollection(IDatabase database)
    {
        var collection = database.GetCollection<ChatMessage>();

        var senderIndex = Builders<ChatMessage>.IndexKeys.Ascending(cm => cm.SenderId);
        var groupIndex = Builders<ChatMessage>.IndexKeys.Ascending(cm => cm.GroupId);
        var sendAtIndex = Builders<ChatMessage>.IndexKeys.Descending(cm => cm.SendAt);

        CreateIndexModel<ChatMessage> index1 = new(senderIndex, new CreateIndexOptions
        {
            Name = "messageSenderIndex"
        });

        CreateIndexModel<ChatMessage> index2 = new(groupIndex, new CreateIndexOptions
        {
            Name = "messageGroupIndex"
        });

        CreateIndexModel<ChatMessage> index3 = new(groupIndex, new CreateIndexOptions
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