using commons.Protos;
using FastEndpoints;
using http.Auth;
using commons;

namespace http.Endpoints;

public class CampusEndpoint<TReq>(ILogger logger) : CampusEndpointBase<TReq>(logger) where TReq : notnull
{
    protected async Task SendAsync(MessageResponse response, CancellationToken cancellationToken)
    {
        if(!response.Success)
        {
            await HandleErrorsAsync(response, cancellationToken);
            return;
        }

        await Send.OkAsync(response.Payload, cancellationToken);
    }
}

public class CampusEndpointBase<TReq>(ILogger logger) : Endpoint<TReq, MessageBody> where TReq : notnull
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
                await Send.ResultAsync(Results.Problem(
                    statusCode: 400,
                    detail: err,
                    title: "Bad Request")
                    );
                return;
            case 401:
                logger.LogDebug("401: {error}", err);
                await Send.ResultAsync(Results.Problem(
                    statusCode: 401,
                    detail: err,
                    title: "Unauthorized")
                    );
                return;
            case 403:
                logger.LogDebug("403: {error}", err);
                await Send.ResultAsync(Results.Problem(
                    statusCode: 403,
                    detail: err,
                    title: "Forbidden")
                    );
                return;
            case 404:
                logger.LogDebug("404: {error}", err);
                await Send.ResultAsync(Results.Problem(
                    statusCode: 404,
                    detail: err,
                    title: "Not Found")
                    );
                return;
            case 500:
                logger.LogDebug("500: {error}", err);
                await Send.ResultAsync(Results.Problem(
                    statusCode: 500,
                    detail: err,
                    title: "Unhandled Error")
                    );
                return;
        }
    }
}
