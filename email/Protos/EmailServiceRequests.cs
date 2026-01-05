using commons.RequestBase;

namespace emailServiceClient;

public partial class SendEmailRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(ToName) || string.IsNullOrEmpty(ToEmail) || string.IsNullOrEmpty(TemplateName) || string.IsNullOrEmpty(TemplateData))
        {
            return "Send Email request is empty";
        }
        return null;
    }
}
