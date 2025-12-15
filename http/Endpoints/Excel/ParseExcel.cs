using commons.Protos;
using excelServiceClient;
using FastEndpoints;
using Google.Protobuf;
using http.Auth;

namespace http.Endpoints.Excel;

public class ParseExcel(ILogger<ParseExcel> logger) : CampusEndpoint<ExcelParseRequest>(logger)
{
    public excelService.excelServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/excel/parse");

        Policies(CampusPolicy.AuthenticatedUser);
        Roles("management", "professor");
    }

    public override async Task HandleAsync(ExcelParseRequest req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrEmpty(req.FileName))
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        var grpcRequest = new ParseExcelRequest
        {
            FileName = req.FileName
        };

        MessageResponse grpcResponse = await Client.ParseExcelAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}
