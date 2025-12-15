using commons.Protos;
using emailServiceClient;
using Grpc.Core;
using MediatR;

namespace email;

public class EmailService(IMediator mediator) : emailService.emailServiceBase
{
    private readonly IMediator _mediator = mediator;

    public override async Task<MessageResponse> SendEmail(SendEmailRequest request, ServerCallContext context)
    {
        return await _mediator.Send(request);
    }
}
