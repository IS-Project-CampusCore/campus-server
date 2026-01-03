using chatServiceClient;
using Microsoft.AspNetCore.SignalR;

namespace notification.Hubs;

public class ChatHub(
    chatService.chatServiceClient chatService,
    ILogger<ChatHub> logger
    ) : Hub
{
    private readonly chatService.chatServiceClient _chatService = chatService;
    private readonly ILogger<ChatHub> _logger = logger;

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value;
        if (userId is not null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            _logger.LogInformation($"User:{userId} was connected to the private channel");

            var response = await _chatService.GetUserGroupsAsync(new GetGroupsRequest { MemberId = userId });
            if (response.Success)
            {
                var payload = response.Payload;
                var groupIds = payload.Array().IterateStrings();

                foreach (var groupId in groupIds)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
                    _logger.LogInformation($"User:{userId} was connected to the Group:{groupId}");
                }
            }
            else
            {
                _logger.LogInformation($"An error ocurred when finding groups:{response.Errors}");
            }
        }

        await base.OnConnectedAsync();
    }
}
