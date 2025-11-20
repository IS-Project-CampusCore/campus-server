using commons;

namespace SomeService;

public class ServiceImplementation
{
    public string ProcessMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new BadRequestException("Message argument can not be null");
        }
        return "The processed message is: " + message;
    }
}
