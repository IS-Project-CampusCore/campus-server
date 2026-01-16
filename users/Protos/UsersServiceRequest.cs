using commons.RequestBase;
using users.Model;

namespace usersServiceClient;

public partial class UserIdRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(Id) ? "User Id can not be empty." : null;
}

public partial class UsersRoleRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(Role) ? "User Role can not be empty." : null;
}
public partial class UsersUniversityRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(University) ? "User University can not be empty." : null;
}

public partial class LoginRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            return "Login Request can not be empty.";
        return null;
    }
}

public partial class RegisterRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Role))
            return "Register Request can not be empty.";
        if (User.StringToRole(Role) == UserType.NO_ROLE)
            return "Register Request role is invalid";
        return null;
    }
}

public partial class VerifyRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(Code))
            return "Verify Request can not be empty.";
        return null;
    }
}
public partial class ResendVerifyCodeRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(Email) ? "Email cannot be empty." : null;
}
public partial class RegisterFromExcelRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(FileName) ? "File Name can not be empty" : null;
}
public partial class UpdateFromExcelRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(FileName) ? "File Name can not be empty" : null;
}
public partial class DeleteAccountRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(UserId) ? "User Id cannot be empty." : null;
}
public partial class ResetPasswordRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(Email) ? "Email cannot be empty." : null;
}
public partial class GetAllUsersRequest : IRequestBase
{
    public string? Validate() => null;
}

public partial class UpdateUserRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(Email) ? "Email cannot be null." : null;
}