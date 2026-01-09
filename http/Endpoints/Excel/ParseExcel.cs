using commons.Protos;
using excelServiceClient;
using FastEndpoints;
using Google.Protobuf;
using http.Auth;

namespace http.Endpoints.Excel;

public class ExcelParseRequest
{
    public string FileName { get; set; } = default!;
    public List<string> CellTypes { get; set; } = new(); 
}

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

        if (req.CellTypes != null && req.CellTypes.Count > 0)
        {
            grpcRequest.CellTypes.AddRange(req.CellTypes);
        }

        MessageResponse grpcResponse = await Client.ParseExcelAsync(grpcRequest, null, null, cancellationToken);

        await SendAsync(grpcResponse, cancellationToken: cancellationToken);
    }
}