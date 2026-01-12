using Announcements.Model;
using announcementsServiceClient;
using commons.Database;
using commons.RequestBase;
using commons.Tools;
using emailServiceClient;
using Grpc.Net.Client;
using MongoDB.Driver;
using System.Text.Json;
using usersServiceClient;

namespace announcements.Implementation;

public class AnnouncementServiceImplementation(
    ILogger<AnnouncementServiceImplementation> logger,
    IDatabase database,
    IConfiguration config)
{
    private readonly ILogger<AnnouncementServiceImplementation> _logger = logger;
    private readonly AsyncLazy<IDatabaseCollection<Announcement>> _announcements = new(() => GetCollection(database));

    private readonly string _userServiceUrl = config["Microservices:Users"] ?? "http://users:8080";
    private readonly string _emailServiceUrl = config["Microservices:Email"] ?? "http://email:8080";

    public async Task<Announcement> CreateAsync(CreateAnnouncementRequest request)
    {
        var collection = await _announcements;

        var announcement = new Announcement
        {
            Title = request.Title,
            Message = request.Message,
            Author = request.Author,
            CreatedAt = DateTime.UtcNow,
            LastEditedAt = DateTime.UtcNow
        };

        await collection.InsertAsync(announcement);

        _ = NotifyAllUsers("New Announcement: " + request.Title, request.Message);

        return announcement;
    }

    public async Task<Announcement> EditAsync(EditAnnouncementRequest request)
    {
        var collection = await _announcements;
        var announcement = await collection.GetOneByIdAsync(request.Id);

        if (announcement is null)
        {
            throw new NotFoundException($"Announcement {request.Id} not found");
        }

        announcement.Title = request.NewTitle;
        announcement.Message = request.NewMessage;
        announcement.LastEditedAt = DateTime.UtcNow;

        await collection.ReplaceAsync(announcement);

        _ = NotifyAllUsers("Updated Announcement: " + request.NewTitle, request.NewMessage);

        return announcement;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var collection = await _announcements;
        var announcement = await collection.GetOneByIdAsync(id);

        if (announcement is null)
        {
            throw new NotFoundException($"Announcement {id} not found");
        }

        await collection.DeleteWithIdAsync(id);
        return true;
    }

    private async Task NotifyAllUsers(string subject, string content)
    {
        try
        {
            var users = await FetchAllUsers();
            if (users is null || users.Count == 0) return;

            using var channel = GrpcChannel.ForAddress(_emailServiceUrl);
            var client = new emailService.emailServiceClient(channel);

            foreach (var user in users)
            {
                if (string.IsNullOrEmpty(user.Email)) continue;

                var emailReq = new SendEmailRequest
                {
                    ToEmail = user.Email,
                    ToName = user.Name ?? "Student",
                    TemplateName = "Announcement",
                    TemplateData = JsonSerializer.Serialize(new { Subject = subject, Body = content })
                };

                try
                {
                    await client.SendEmailAsync(emailReq);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send email to {user.Email}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute NotifyAllUsers flow");
        }
    }

    private async Task<List<UserDto>?> FetchAllUsers()
    {
        using var channel = GrpcChannel.ForAddress(_userServiceUrl);
        var client = new usersService.usersServiceClient(channel);

        var response = await client.GetAllUsersAsync(new GetAllUsersRequest { Placeholder = "" });

        if (!response.Success || string.IsNullOrEmpty(response.Body))
        {
            _logger.LogWarning("Could not fetch users: " + response.Errors);
            return [];
        }

        return JsonSerializer.Deserialize<List<UserDto>>(response.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    internal static async Task<IDatabaseCollection<Announcement>> GetCollection(IDatabase database)
    {
        var collection = database.GetCollection<Announcement>();

        var authorIndex = Builders<Announcement>.IndexKeys.Ascending(a => a.Author);

        await collection.MongoCollection.Indexes.CreateManyAsync([
            new CreateIndexModel<Announcement>(authorIndex, new CreateIndexOptions { Name = "AnnouncementAuthorIndex" })
        ]);

        return collection;
    }
}

public class UserDto
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
}