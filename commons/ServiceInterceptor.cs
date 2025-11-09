using commons.Protos;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace commons;

public class ServiceInterceptor(ILogger<ServiceInterceptor> logger) : Interceptor
{
    private readonly ILogger<ServiceInterceptor> _logger = logger;

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext callContext, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        _logger.LogInformation("gRPC request starting method:{Method}", callContext.Method);

        try
        {
            return await continuation(request, callContext);
        }
        catch (ServiceMessageException ex)
        {
            _logger.LogError("Handled service error in method:{Method}", callContext.Method);

            return (TResponse)(object)ex.ToResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled internal error in {Method}", callContext.Method);

            return (TResponse)(object)MessageResponse.Error(ex);
        }
    }
}
