using commons;
using commons.Database;
using commons.RequestBase;
using commons.Tools;
using emailServiceClient;
using usersServiceClient;
using MongoDB.Driver;
using System.Text.Json;
using Announcements.Model;

namespace Announcements.Implementation;

public class AnnouncementsServiceImplementation(
    ILogger<AnnouncementsServiceImplementation> logger,
    IDatabase database,
    usersService.usersServiceClient usersClient,
    emailService.emailServiceClient emailClient
    )
{
    private readonly ILogger<AnnouncementsServiceImplementation> _logger = logger;
    private readonly usersService.usersServiceClient _usersClient = usersClient;
    private readonly emailService.emailServiceClient _emailClient = emailClient;

    private readonly AsyncLazy<IDatabaseCollection<Announcement>> _announcementsCollection = new(() => GetAnnouncementCollection(database));

    public async Task CreateAnnouncement(string title, string message, string authorId)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
        {
            throw new BadRequestException("Title and Message cannot be empty.");
        }

        var db = await _announcementsCollection;

        var newAnnouncement = new Announcement
        {
            Title = title,
            Message = message,
            AuthorId = authorId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        await db.InsertAsync(newAnnouncement);

        _logger.LogInformation($"Announcement '{title}' created by {authorId}");

        // Fire and forget notification (sau await dacă vrei să blochezi până se trimit)
        await NotifyAllUsers($"New Announcement: {title}", message);
    }

    public async Task EditAnnouncement(string id, string newTitle, string newMessage)
    {
        var db = await _announcementsCollection;
        var announcement = await db.GetOneByIdAsync(id);

        if (announcement is null)
        {
            throw new NotFoundException($"Announcement with id {id} not found.");
        }

        announcement.Title = newTitle;
        announcement.Message = newMessage;
        announcement.UpdatedAt = DateTime.UtcNow;

        await db.ReplaceAsync(announcement);

        _logger.LogInformation($"Announcement '{id}' has been updated");

        await NotifyAllUsers($"Updated Announcement: {newTitle}", newMessage);
    }

    public async Task DeleteAnnouncement(string id)
    {
        var db = await _announcementsCollection;

        bool exists = await db.ExistsAsync(a => a.Id == id);

        if (!exists)
        {
            throw new NotFoundException($"Announcement with id {id} not found.");
        }

        await db.DeleteWithIdAsync(id);

        _logger.LogInformation($"Announcement with ID:{id} has been deleted.");
    }

    private async Task NotifyAllUsers(string subject, string content)
    {
        try
        {
            var allUsersResponse = await _usersClient.GetAllUsersAsync("new EmptyRequest()");

            if (allUsersResponse == null || allUsersResponse.Users.Count == 0) return;

            foreach (var user in allUsersResponse.Users)
            {
                string templateDataString = JsonSerializer.Serialize(new
                {
                    Name = user.Name,
                    Title = subject,
                    Message = content
                });

                await _emailClient.SendEmailAsync(new SendEmailRequest
                {
                    ToEmail = user.Email,
                    ToName = user.Name,
                    TemplateName = "Announcement", 
                    TemplateData = templateDataString
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast announcement emails.");
        }
    }

    internal static async Task<IDatabaseCollection<Announcement>> GetAnnouncementCollection(IDatabase database)
    {
        var collection = database.GetCollection<Announcement>();

        var createdAtIndex = Builders<Announcement>.IndexKeys.Descending(a => a.CreatedAt);

        await collection.MongoCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<Announcement>(createdAtIndex, new CreateIndexOptions { Name = "AnnouncementCreatedAtIndex" })
        );

        return collection;
    }
}