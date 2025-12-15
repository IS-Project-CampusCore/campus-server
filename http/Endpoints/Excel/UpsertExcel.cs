using commons.Protos;
using excelServiceClient;
using FastEndpoints;
using Google.Protobuf;
using http.Auth;

namespace http.Endpoints.Excel;

public class UpsertExcel(ILogger<UpsertExcel> logger) : CampusEndpoint<ExcelUplaodRequest>(logger)
{
    public excelService.excelServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/excel/upsert");
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

        var grpcRequest = new UpsertExcelRequest
        {
            FileName = req.File.FileName,
            Content = ByteString.CopyFrom(bytes)
        };

        MessageResponse grpcResponse = await Client.UpsertExcelAsync(grpcRequest, null, null, cancellationToken);
        await SendAsync(grpcResponse, cancellationToken);
    }
}
