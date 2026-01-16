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

    public override async Task<MessageResponse> GetUserById(UserIdRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> GetUsersByRole(UsersRoleRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> GetUsersByUniversity(UsersUniversityRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

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
    public override async Task<MessageResponse> ResendVerifyCode(ResendVerifyCodeRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }
    public override async Task<MessageResponse> RegisterUsersFromExcel(RegisterFromExcelRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }
    public override async Task<MessageResponse> UpdateUsersFromExcel(UpdateFromExcelRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }
    public override async Task<MessageResponse> DeleteAccount(DeleteAccountRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }
    public override async Task<MessageResponse> ResetPassword(ResetPasswordRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }
    public override async Task<MessageResponse> GetAllUsers(GetAllUsersRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }

    public override async Task<MessageResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }
}
