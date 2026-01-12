using commons.Protos;
using FastEndpoints;
using http.Auth;
using commons;
using System.Net;
using System.Text.Json;

namespace http.Endpoints;

public abstract class CampusEndpoint<TReq>(ILogger logger) : CampusEndpointBase<TReq>(logger) where TReq : notnull
{
    protected async Task SendAsync(MessageResponse response, CancellationToken cancellationToken)
    {
        if(!response.Success)
        {
            await HandleErrorsAsync(response, cancellationToken);
            return;
        }

        await Send.OkAsync(response.Payload.Json, cancellationToken);
    }
}

public abstract class CampusEndpoint(ILogger logger) : CampusEndpointBase<EmptyRequest>(logger)
{
    protected async Task SendAsync(MessageResponse response, CancellationToken cancellationToken)
    {
        if (!response.Success)
        {
            await HandleErrorsAsync(response, cancellationToken);
            return;
        }

        await Send.OkAsync(response.Payload.Json, cancellationToken);
    }
}

public abstract class CampusEndpointBase<TReq>(ILogger logger) : Endpoint<TReq, JsonElement> where TReq : notnull
{
    protected void AllowUnverifiedUser()
    {
        Policies(CampusPolicy.UnverifiedUser);
    }

    protected string GetUserId()
    {
        var userId = HttpContext.User.Claims.SingleOrDefault(c => c.Type == UserJwtExtensions.IdClaim)
            ?? throw new Exception("User is not authenticated");

        return userId.Value;
    }

    protected string GetUserRole()
    {
        var userRole = HttpContext.User.Claims.SingleOrDefault(c => c.Type == UserJwtExtensions.RoleClaim)
            ?? throw new Exception("User is not authenticated");

        return userRole.Value;
    }

    protected async Task<bool> HandleErrorsAsync(MessageResponse response, CancellationToken cancellationToken)
    {
        if (response.Success)
            return true;

        await HandleErrorsAsync(response.Code, response.Errors, cancellationToken);
        return false;
    }

    protected async Task HandleErrorsAsync(int code, string? err, CancellationToken cancellationToken)
    {
        switch (code)
        {
            case 400:
                logger.LogDebug("400: {error}", err);
                AddError(err ?? "Bad request", ((HttpStatusCode)400).ToString());
                await Send.ErrorsAsync(400, cancellationToken);
                return;
            case 401:
                logger.LogDebug("401: {error}", err);
                await Send.UnauthorizedAsync(cancellationToken);
                return;
            case 403:
                logger.LogDebug("403: {error}", err);
                await Send.ForbiddenAsync(cancellationToken);
                return;
            case 404:
                logger.LogDebug("404: {error}", err);
                await Send.NotFoundAsync(cancellationToken);
                return;
            case 500:
                logger.LogDebug("500: {error}", err);
                AddError(err ?? "Internal rrror", ((HttpStatusCode)500).ToString());
                await Send.ErrorsAsync(500, cancellationToken);
                return;
        }
    }
}
