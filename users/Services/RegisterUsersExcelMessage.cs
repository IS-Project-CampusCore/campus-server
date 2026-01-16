using commons.Protos;
using commons.RequestBase;
using MediatR;
using users.Implementation;
using users.Model;
using usersServiceClient;

namespace users.Services;

public class RegisterUserExcelMessage(
    ILogger<RegisterUserExcelMessage> logger,
    IUsersServiceImplementation implementation
) : CampusMessage<RegisterFromExcelRequest, BulkResponse>(logger)
{
    private readonly IUsersServiceImplementation _implementation = implementation;

    protected override async Task<BulkResponse> HandleMessage(RegisterFromExcelRequest request, CancellationToken token) =>
        await _implementation.BulkRegisterAsync(request.FileName);
}