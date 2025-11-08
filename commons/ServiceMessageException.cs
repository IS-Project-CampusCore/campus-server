using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.AspNetCore;
using Grpc.Core;


namespace commons;

public class ServiceMessageException(StatusCode statusCode, string message) : RpcException(new Status(statusCode, message), message);

public class BadRequestException(string message) : ServiceMessageException(StatusCode.InvalidArgument, message);
public class UnauthorizedException(string message) : ServiceMessageException(StatusCode.Unauthenticated, message);
public class ForbiddenException(string message) : ServiceMessageException(StatusCode.PermissionDenied, message);
public class InternalErrorException(string message) : ServiceMessageException(StatusCode.Internal, message);
