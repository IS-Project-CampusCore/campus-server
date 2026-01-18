using commons.RequestBase;

namespace campusServiceClient;

public partial class GenerateDistributionRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(Placeholder))
            return "Empty Request Message";
        return null;
    }
}
public partial class CreatePaymentRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(UserId))
            return "Empty Request Message";
        return null;
    }
}
public partial class ReportIssueRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(IssuerId))
            return "Empty Request Message";
        return null;
    }
}
public partial class CreateAccommodationRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(Name))
            return "Empty Request Message";
        return null;
    }
}

public partial class GetAccByIdRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(Id) ? "Get Accomodation by Id cannot be empty" : null;
}

public partial class GetAccsRequest : IRequestBase
{
    public string? Validate() => null;
}
