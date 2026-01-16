using campusServiceClient;
using http.Auth;
using commons.Protos;

namespace http.Endpoints.Campus;

public record CardData(string CardNumber, string ExpDate, string CVV);

public record CreatePaymentApiRequest(double Amount, CardData CardData);

public class CreatePayment(ILogger<CreatePayment> logger) : CampusEndpoint<CreatePaymentApiRequest>(logger)
{
    public campusService.campusServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/campus/create-payment");
        Policies(CampusPolicy.AuthenticatedUser);

        Roles("campus_student");
    }

    public override async Task HandleAsync(CreatePaymentApiRequest req, CancellationToken cancellationToken)
    {
        if (req is null || req.Amount <= 0)
        {
            await HandleErrorsAsync(400, "Amount must be greater than 0", cancellationToken);
            return;
        }

        if (req.CardData is null
            || string.IsNullOrWhiteSpace(req.CardData.CardNumber)
            || string.IsNullOrWhiteSpace(req.CardData.ExpDate)
            || string.IsNullOrWhiteSpace(req.CardData.CVV))
        {
            await HandleErrorsAsync(400, "CardData must include CardNumber, ExpDate and CVV", cancellationToken);
            return;
        }

        var userId = GetUserId();

        // NOTE: campusService.CreatePaymentRequest currently only accepts UserId and Amount.
        // If you want to forward CardData to the campus service, update campusService.proto and server implementation.
        var grpcRequest = new CreatePaymentRequest
        {
            UserId = userId,
            Amount = req.Amount
        };

        // Optional: log receipt of card data (do NOT log sensitive card numbers in production)
        _ = logger; // to avoid unused variable warning in some analyzers
        logger.LogDebug("Received payment request for user {UserId} with card ending {Last4}", userId, req.CardData.CardNumber[^4..]);

        MessageResponse grpcResponse = await Client.CreatePaymentAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}