using FastEndpoints;
using excelServiceClient;
using commons.Protos;
using Google.Protobuf;

namespace http.Endpoints;
public class ExcelUplaodRequest
{
    public IFormFile File {get; set;} = default!;
}   

public class ExcelEndpoint : Endpoint<ExcelUplaodRequest, string>
{
    public excelService.excelServiceClient excelServiceClient { get; set; } = default!;

    public override void Configure()
    {
        Post("api/excel/upload");
    }

    public override async Task HandleAsync(ExcelUplaodRequest req, CancellationToken ct)
    {
        if (req.File is null || req.File.Length == 0)
        {
            await Send.ErrorsAsync(400, cancellation: ct);
            return;
        }

        await using var ms = new MemoryStream();
        await req.File.CopyToAsync(ms, ct);
        var bytes = ms.ToArray();

        var grpcRequest = new CheckOrUpdateExcelRequest
        {
            FileName = req.File.FileName,
            Content = ByteString.CopyFrom(bytes)
        };

        MessageResponse grpcResponse;

        try
        {
            grpcResponse = await excelServiceClient.CheckOrUpdateAsync(grpcRequest, cancellationToken: ct);
        }
        catch (Exception)
        {
            await Send.ErrorsAsync(500, cancellation: ct);
            return;
        }

        if (grpcResponse.Success)
        {
            await Send.OkAsync(grpcResponse.Body?.ToString() ?? "{}", cancellation: ct);
        }
        else
        {
            await Send.ErrorsAsync(grpcResponse.Code, cancellation: ct);
        }
    }
}

