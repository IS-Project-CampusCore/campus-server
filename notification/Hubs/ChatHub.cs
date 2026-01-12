using chatServiceClient;
using Microsoft.AspNetCore.Authorization;
using notification.Auth;
using commons.SignalRBase;

namespace notification.Hubs;

[Authorize(Policy = ChatPolicy.AuthenticatedUser)]
public class ChatHub(
    ILogger<ChatHub> logger,
    IConnectionMapping<ChatHub> connectionMapping,
    chatService.chatServiceClient chatService
    ) : CampusHub<ChatHub>(logger, connectionMapping)
{
    private readonly chatService.chatServiceClient _chatService = chatService;

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        if (CurrentUser is not null)
        {
            var response = await _chatService.GetUserGroupsAsync(new GetUserGroupsRequest { MemberId = CurrentUser.Id });
            if (!response.Success)
            {
                _logger.LogInformation($"An error ocurred when finding groups:{response.Errors}");
                return;
            }

            var payload = response.Payload;
            var groups = payload.TryArray()?.Iterate().Select(e => new
            {
                Id = e.GetString("Id"),
                Name = e.GetString("Name")
            });

            if (groups is not null)
            {
                foreach (var group in groups)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, group.Id);
                    _logger.LogInformation($"User:{CurrentUser.Name} was connected to the Group:{group.Name}");
                }
            }
            else
            {
                _logger.LogInformation($"User:{CurrentUser.Name} has no groups, single connection to private chanel");
            }
        }
    }
}