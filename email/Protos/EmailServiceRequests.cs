using commons.Protos;
using MediatR;

namespace emailServiceClient;

public partial class SendEmailRequest : IRequest<MessageResponse>;
