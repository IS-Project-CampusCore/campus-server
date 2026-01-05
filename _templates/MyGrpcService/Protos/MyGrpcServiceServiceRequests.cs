using commons.RequestBase;

namespace __CAMEL_NAME__ServiceClient;

public partial class ExampleRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(Message))
            return "Empty Request Message";
        return null;
    }
}
