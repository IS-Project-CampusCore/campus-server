using commons.RequestBase;

namespace scheduleServiceClient;

public partial class UpsertUniScheduleRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(FileName) || string.IsNullOrEmpty(University) || string.IsNullOrEmpty(Major) || Year < 1 || Groups < 1)
            return "Upsert Uni Schedule cannot be empty";
        return null;
    }
}

public partial class UpsertAccScheduleRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(FileName) || string.IsNullOrEmpty(Name))
            return "Upsert Acc Schedule cannot be empty";
        return null;
    }
}

public partial class GetUniScheduleRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(University) || string.IsNullOrEmpty(Major) || Year < 0 || Group < 0)
            return "Get Uni Schedule cannot be empty";
        return null;
    }
}

public partial class GetAccScheduleRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(Name))
            return "Get Acc Schedule cannot be empty";
        return null;
    }
}
