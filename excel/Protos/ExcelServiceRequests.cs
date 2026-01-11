using commons.Protos;
using commons.RequestBase;
using MediatR;
using Serilog.Sinks.File;

namespace excelServiceClient;

public partial class InsertExcelRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(FileName) || Content is null || Content.IsEmpty)
        {
            return "Insert Excel Request is empty";
        }
        return null;
    }
}

public partial class UpdateExcelRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(FileName) || Content is null || Content.IsEmpty)
        {
            return "Update Excel Request is empty";
        }
        return null;
    }
}

public partial class UpsertExcelRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(FileName) || Content is null || Content.IsEmpty)
        {
            return "Upsert Excel Request is empty";
        }
        return null;
    }
}

public partial class ParseExcelRequest : IRequestBase
{
    public string? Validate()
    {
        if(string.IsNullOrEmpty(FileName) || (CellTypes is null || CellTypes.Count == 0))
        {
            return  "Parse Excel Request requiers a file name";
        }
        return null;
    } 
    

}