 public record CampusStudent
        (string id,
        string name,
        string email,
        string passwordHash,
        bool isVerified,
        string university,
        int year,
        int group,
        string major,
        string campusNr, 
        string roomNr
        ): Student(id,name,email,passwordHash,isVerified,university,year,group,major);
