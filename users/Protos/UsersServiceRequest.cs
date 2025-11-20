using commons.Protos;
using MediatR;

namespace usersServiceClient;

public partial class LoginRequest : IRequest<MessageResponse>;

public partial class RegisterRequest : IRequest<MessageResponse>;

public partial class VerifyRequest : IRequest<MessageResponse>;
