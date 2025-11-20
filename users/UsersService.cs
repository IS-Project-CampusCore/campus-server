using commons;
using commons.Protos;
using users.Services;
using usersServiceClient;
using Grpc.Core;
using MediatR;

namespace users;

public class UsersService(IMediator mediator) : usersService.usersServiceBase
{
    private readonly IMediator _mediator = mediator;

    public override async Task<MessageResponse> Login(LoginRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> Register(RegisterRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> Verify(VerifyRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }
}
