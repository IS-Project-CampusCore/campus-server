using commons.Protos;
using campusServiceClient;
using FastEndpoints;
using http.Auth;

namespace http.Endpoints.Campus;

public class CreateAccommodationReq
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int OpenTime { get; set; } = default!;
    public int CloseTime { get; set; } = default!;
}

public class CreateAccommodation(ILogger<CreateAccommodation> logger) : CampusEndpoint<CreateAccommodationReq>(logger)
{
    public campusService.campusServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/campus/create-accommodation");
        Policies(CampusPolicy.AuthenticatedUser);

        Roles("management");
    }

    public override async Task HandleAsync(CreateAccommodationReq req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrWhiteSpace(req.Name))
        {
            await HandleErrorsAsync(400, "Accommodation Name is required", cancellationToken);
            return;
        }

        var grpcRequest = new CreateAccommodationRequest
        {
            Name = req.Name,
            Description = req.Description ?? string.Empty,
            OpenTime = req.OpenTime,
            CloseTime = req.CloseTime
        };

        MessageResponse grpcResponse = await Client.CreateAccommodationAsync(grpcRequest, null, null, cancellationToken);

        await SendAsync(grpcResponse, cancellationToken: cancellationToken);
    }
}