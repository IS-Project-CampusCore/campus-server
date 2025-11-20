using commons;
using commons.Protos;
using Grpc.Core;
using MediatR;
using users.Model;
using usersServiceClient;

namespace users.Services;

public class VerifyMessage(
    ILogger<LoginMessage> logger,
    IUsersServiceImplementation implementation
) : IRequestHandler<VerifyRequest, MessageResponse>
{
    private readonly ILogger<LoginMessage> _logger = logger;
    private readonly IUsersServiceImplementation _implementation = implementation;

    public Task<MessageResponse> Handle(VerifyRequest request, CancellationToken token)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.Code))
        {
            _logger.LogError("Request filed can not be empty");
            throw new BadRequestException("Request filed can not be empty");
        }

        try
        {
            var email = request.Email;
            var password = request.Password;
            var code = request.Code;

            return Task.FromResult(MessageResponse.Ok(_implementation.Verify(email, password, code)));
        }
        catch (BadRequestException ex)
        {
            return Task.FromResult(MessageResponse.BadRequest(ex.Message));
        }
        catch (Exception ex)
        {
            return Task.FromResult(MessageResponse.Error(ex.Message));
        }
    }
}