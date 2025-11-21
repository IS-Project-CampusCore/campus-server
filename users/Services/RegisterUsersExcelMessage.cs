using commons;
using commons.Protos;
using MediatR;
using users.Model;
using usersServiceClient;

namespace users.Services;

public class RegisterUserExcelMessage(
    ILogger<LoginMessage> logger,
    IUsersServiceImplementation implementation
) : IRequestHandler<UsersExcelRequest, MessageResponse>
{
    private readonly ILogger<LoginMessage> _logger = logger;
    private readonly IUsersServiceImplementation _implementation = implementation;

    public async Task<MessageResponse> Handle(UsersExcelRequest request, CancellationToken token)
    {
        if (string.IsNullOrEmpty(request.FileName))
        {
            _logger.LogError("Request filed can not be empty");
            throw new BadRequestException("Request filed can not be empty");
        }

        try
        {
            var fileName = request.FileName;

            return MessageResponse.Ok(await _implementation.RegisterUsersFromExcel(fileName));
        }
        catch (BadRequestException ex)
        {
            return MessageResponse.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return MessageResponse.Error(ex.Message);
        }
    }
}