using commons.RequestBase;
using users.Implementation;
using users.Model;
using usersServiceClient;

namespace users.Services;

public class UpdateUsersExcelMessage(
    ILogger<UpdateUsersExcelMessage> logger,
    IUsersServiceImplementation implementation
) : CampusMessage<UpdateFromExcelRequest, BulkResponse>(logger)
{
    private readonly IUsersServiceImplementation _implementation = implementation;

    protected override async Task<BulkResponse> HandleMessage(UpdateFromExcelRequest request, CancellationToken token) =>
        await _implementation.BulkUpdateAsync(request.FileName);
}
