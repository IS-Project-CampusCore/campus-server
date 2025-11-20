using commons.Protos;
using excelServiceClient;
using FastEndpoints;
using Google.Protobuf;
using System.Text.Json.Nodes;

namespace http.Endpoints.Excel;

public record ExcelUplaodRequest(IFormFile File);

public record ExcelParseRequest(string FileName);

public class InsertExcel : Endpoint<ExcelUplaodRequest, MessageResponse>
{
    public excelService.excelServiceClient excelServiceClient { get; set; } = default!;

    public override void Configure()
    {
        Post("api/excel/insert");
        AllowFileUploads();
        AllowAnonymous();
    }

    public override async Task HandleAsync(ExcelUplaodRequest req, CancellationToken cancellationToken)
    {
        if (req.File is null || req.File.Length == 0)
        {
            await Send.ErrorsAsync(400, cancellation: cancellationToken);
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

        try
        {
            MessageResponse grpcResponse = await excelServiceClient.InsertExcelAsync(grpcRequest, null, null, cancellationToken);

            if (grpcResponse.Success)
            {
                await Send.OkAsync(grpcResponse);
                return;
            }

            await Send.ErrorsAsync(grpcResponse.Code);
        }
        catch (Exception)
        {
            await Send.ErrorsAsync(500);
        }
    }
}

public class UpdateExcel : Endpoint<ExcelUplaodRequest, MessageResponse>
{
    public excelService.excelServiceClient excelServiceClient { get; set; } = default!;

    public override void Configure()
    {
        Post("api/excel/update");
        AllowFileUploads();
        AllowAnonymous();
    }

    public override async Task HandleAsync(ExcelUplaodRequest req, CancellationToken cancellationToken)
    {
        if (req.File is null || req.File.Length == 0)
        {
            await Send.ErrorsAsync(400, cancellation: cancellationToken);
            return;
        }

        using MemoryStream ms = new();
        await req.File.CopyToAsync(ms, cancellationToken);

        byte[] bytes = ms.ToArray();

        var grpcRequest = new UpdateExcelRequest
        {
            FileName = req.File.FileName,
            Content = ByteString.CopyFrom(bytes)
        };

        try
        {
            MessageResponse grpcResponse = await excelServiceClient.UpdateExcelAsync(grpcRequest, null, null, cancellationToken);

            if (grpcResponse.Success)
            {
                await Send.OkAsync(grpcResponse);
                return;
            }

            await Send.ErrorsAsync(grpcResponse.Code);
        }
        catch (Exception)
        {
            await Send.ErrorsAsync(500);
        }
    }
}

public class UpsertExcel : Endpoint<ExcelUplaodRequest, MessageResponse>
{
    public excelService.excelServiceClient excelServiceClient { get; set; } = default!;

    public override void Configure()
    {
        Post("api/excel/upsert");
        AllowFileUploads();
        AllowAnonymous();
    }

    public override async Task HandleAsync(ExcelUplaodRequest req, CancellationToken cancellationToken)
    {
        if (req.File is null || req.File.Length == 0)
        {
            await Send.ErrorsAsync(400, cancellation: cancellationToken);
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

        try
        {
            MessageResponse grpcResponse = await excelServiceClient.UpsertExcelAsync(grpcRequest, null, null, cancellationToken);

            if (grpcResponse.Success)
            {
                await Send.OkAsync(grpcResponse);
                return;
            }

            await Send.ErrorsAsync(grpcResponse.Code);
        }
        catch (Exception)
        {
            await Send.ErrorsAsync(500);
        }
    }
}

public class ParseExcel : Endpoint<ExcelParseRequest, MessageResponse>
{
    public excelService.excelServiceClient excelServiceClient { get; set; } = default!;

    public override void Configure()
    {
        Post("api/excel/parse");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ExcelParseRequest req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrEmpty(req.FileName))
        {
            await Send.ErrorsAsync(400, cancellation: cancellationToken);
            return;
        }

        var grpcRequest = new ParseExcelRequest
        {
            FileName = req.FileName,
        };

        try
        {
            MessageResponse grpcResponse = await excelServiceClient.ParseExcelAsync(grpcRequest, null, null, cancellationToken);

            if (grpcResponse.Success)
            {
                await Send.OkAsync(grpcResponse);
                return;
            }

            await Send.ErrorsAsync(grpcResponse.Code);
        }
        catch (Exception)
        {
            await Send.ErrorsAsync(500);
        }
    }
}