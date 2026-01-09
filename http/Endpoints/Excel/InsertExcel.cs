using commons.Protos;
using excelServiceClient;
using FastEndpoints;
using Google.Protobuf;
using http.Auth;

namespace http.Endpoints.Excel;


//public record ExcelParseRequest(string FileName);
public record ExcelUplaodRequest(IFormFile File);

public class InsertExcel(ILogger<InsertExcel> logger) : CampusEndpoint<ExcelUplaodRequest>(logger)
{
    public excelService.excelServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/excel/insert");
        AllowFileUploads();

        Policies(CampusPolicy.AuthenticatedUser);
        Roles("management", "professor");
    }

    public override async Task HandleAsync(ExcelUplaodRequest req, CancellationToken cancellationToken)
    {
        if (req.File is null || req.File.Length == 0)
        {
            await HandleErrorsAsync(400, "Empty request", cancellationToken);
            return;
        }

        using MemoryStream ms = new();
        await req.File.CopyToAsync(ms, cancellationToken);

        byte[] bytes = ms.ToArray();

        var grpcRequest = new InsertExcelRequest
        {
            FileName = req.File.FileName,
            Content = ByteString.CopyFrom(bytes)
        };

        MessageResponse grpcResponse = await Client.InsertExcelAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}