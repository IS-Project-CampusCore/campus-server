using commons;
using FastEndpoints;
using Grpc.Net.Client;
using someServiceClient;

namespace http.Endpoints;

public record DoSomethingRequest(string message);
public record DoSomethingResponse(string message);

public class DoSomething : Endpoint<DoSomethingRequest, MessageResponse>
{
    public someService.someServiceClient Client { get; set; } = default!;
    public override void Configure()
    {
        Post("api/do-something");
        AllowAnonymous();
    }
    public override async Task HandleAsync(DoSomethingRequest request, CancellationToken cancellationToken)
    {
        MessageResponse res = new MessageResponse();
        try
        {
            var apiRes = await Client.DoSomethingAsync(new DoSomethingReq { SomeReqMessage = request.message }, cancellationToken: cancellationToken);

            res = MessageResponse.FromProtoMessage(apiRes);
        }
        catch(BadRequestException)
        {
            await Send.ErrorsAsync();
        }

        await Send.OkAsync(res, cancellationToken);
    }
}
