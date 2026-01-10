using commons.RequestBase;

namespace announcementsServiceClient;

public partial class ExampleRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(""))
            return "Empty Request Message";
        return null;
    }
}

