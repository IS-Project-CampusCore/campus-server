using commons.Protos;
using campusServiceClient;
using FastEndpoints;
using http.Auth;

namespace http.Endpoints.Campus;

public class GenerateDistributionReq
{
    public string Placeholder { get; set; } = default!;
}

public class GenerateDistribution(ILogger<GenerateDistribution> logger) : CampusEndpoint<GenerateDistributionReq>(logger)
{
    public campusService.campusServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/campus/generate-distribution");
        Policies(CampusPolicy.AuthenticatedUser);
        Roles("management", "professor");
    }

    public override async Task HandleAsync(GenerateDistributionReq req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrEmpty(req.Placeholder))
        {
            await HandleErrorsAsync(400, "Empty request or missing Placeholder", cancellationToken);
            return;
        }

        var grpcRequest = new GenerateDistributionRequest
        {
            Placeholder = req.Placeholder
        };

        MessageResponse grpcResponse = await Client.GenerateDistributionAsync(grpcRequest, null, null, cancellationToken);

        await SendAsync(grpcResponse, cancellationToken: cancellationToken);
    }
}