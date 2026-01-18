using commons.RequestBase;

namespace announcementsServiceClient;

public partial class CreateAnnouncementRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(Title) || string.IsNullOrEmpty(Message))
        {
            return "Announcement must have a title and a message";
        }
        return null;
    }
}

public partial class EditAnnouncementRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(NewTitle) || string.IsNullOrEmpty(NewMessage))
        {
            return "Edit request requires Id, Title, and Message";
        }
        return null;
    }
}

public partial class DeleteAnnouncementRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(Id))
        {
            return "Delete request requires an Id";
        }
        return null;
    }
}

public partial class GetAnnRequest : IRequestBase
{
    public string? Validate() => null;
}