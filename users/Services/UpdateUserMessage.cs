using commons.RequestBase;
using users.Implementation;
using users.Model;
using usersServiceClient;

namespace users.Services;

public class UpdateUserMessage(
    ILogger<LoginMessage> logger,
    IUsersServiceImplementation implementation
) : CampusMessage<UpdateUserRequest, User?>(logger)
{
    private readonly IUsersServiceImplementation _implementation = implementation;

    protected override async Task<User?> HandleMessage(UpdateUserRequest request, CancellationToken token)
    {
        var name = request.HasName ? request.Name : null;
        UserType? role = request.HasRole ? User.StringToRole(request.Role) : null;
        var university = request.HasUniversity ? request.University : null;
        int? year = request.HasYear ? request.Year : null;
        int? group = request.HasGroup ? request.Group : null;
        var major = request.HasMajor ? request.Major : null;
        var dormitory = request.HasDormitory ? request.Dormitory : null;
        string? room = request.HasRoom ? request.Room : null;
        var department = request.HasDepartment ? request.Department : null;
        var title = request.HasTitle ? request.Title : null;

        return await _implementation.UpdateUserAsync(request.Email, name, role, university, year, group, major, dormitory, room, department, title);
    }
}
