using commons.Protos;
using commons.RequestBase;
using MediatR;
using users.Implementation;
using users.Model;
using usersServiceClient;

namespace users.Services;

public class RegisterUserExcelMessage(
    ILogger<LoginMessage> logger,
    IUsersServiceImplementation implementation
) : CampusMessage<UsersExcelRequest, BulkRegisterResponse>(logger)
{
    private readonly IUsersServiceImplementation _implementation = implementation;

    protected override async Task<BulkRegisterResponse> HandleMessage(UsersExcelRequest request, CancellationToken token) =>
        await _implementation.BulkRegisterAsync(request.FileName);
}