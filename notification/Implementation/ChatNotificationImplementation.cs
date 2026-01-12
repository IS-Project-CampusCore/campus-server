using chatServiceClient;
using commons.EventBase;
using commons.SignalRBase;
using emailServiceClient;
using notification.Hubs;
using System.Text.Json;
using usersServiceClient;

namespace Notification.Implementation;

public class ChatNotificationImplementation(
    ILogger<ChatNotificationImplementation> logger,
    IConnectionMapping<ChatHub> connectionMapping,
    chatService.chatServiceClient chatService,
    usersService.usersServiceClient usersService,
    emailService.emailServiceClient emailService
)
{
    private readonly ILogger<ChatNotificationImplementation> _logger = logger;
    private readonly IConnectionMapping<ChatHub> _connectionMapping = connectionMapping;

    private readonly chatService.chatServiceClient _chatService = chatService;
    private readonly usersService.usersServiceClient _usersService = usersService;
    private readonly emailService.emailServiceClient _emailService = emailService;

    public async Task SendBroadcastNotification(string groupId, string senderIdToSkip, string emailTemplateName, string emailTemplateData)
    {
        var groupMembersRes = await _chatService.GetGroupMembersAsync(new GetGroupMembersRequest { GroupId = groupId });
        if (!groupMembersRes.Success)
        {
            _logger.LogError($"Get Group Members failed with Code:{groupMembersRes.Code}, Errors:{groupMembersRes.Errors}");
            throw new EventErrorException(groupMembersRes.Errors);
        }

        var membersPayload = groupMembersRes.Payload;
        IEnumerable<string> memberIds = membersPayload.Array().IterateStrings();

        foreach (var memberId in memberIds)
        {
            if (senderIdToSkip == memberId)
                continue;

            if (!IsUserOnline(memberId))
            {
                await SendEmailToMember(groupId, memberId, emailTemplateName, emailTemplateData);
            }
        }
    }

    public async Task SendTargetedNotification(string groupId, string targetedUserId, string emailTemplateName, string emailTemplateData)
    {
        if (!IsUserOnline(targetedUserId)) {
            await SendEmailToMember(groupId, targetedUserId, emailTemplateName, emailTemplateData);
        }
    }

    public async Task SendEmailToMember(string groupId, string memberId, string emailTemplateName, string emailTemplateData)
    {
        var userRes = await _usersService.GetUserByIdAsync(new UserIdRequest { Id = memberId });
        if (!userRes.Success)
        {
            _logger.LogInformation($"Get User by Id failed with Code:{userRes.Code}, Errors:{userRes.Errors}");
            return;
        }

        var userPayload = userRes.Payload;
        var userEmail = userPayload.GetString("Email");
        var userName = userPayload.GetString("Name");

        var emailRes = await _emailService.SendEmailAsync(new SendEmailRequest
        {
            ToEmail = userEmail,
            ToName = userName,
            TemplateName = emailTemplateName,
            TemplateData = emailTemplateData
        });

        if (!emailRes.Success)
        {
            _logger.LogInformation($"Send Email failed with Code:{emailRes.Code}, Errors:{emailRes.Errors}");
            return;
        }

        _logger.LogInformation($"Email Notification sent to Email:{userEmail}");
    }

    protected bool IsUserOnline(string userId)
    {
        return _connectionMapping.GetConnections(userId).Any();
    }
}
