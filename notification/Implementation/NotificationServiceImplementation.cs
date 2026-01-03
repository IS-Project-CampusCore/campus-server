namespace Notification.Implementation;

public class NotificationServiceImplementation(
    ILogger<NotificationServiceImplementation> logger
)
{
    private readonly ILogger<NotificationServiceImplementation> _logger = logger;
}
