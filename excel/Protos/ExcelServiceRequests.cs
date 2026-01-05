using commons.Protos;
using commons.RequestBase;
using MediatR;

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
    public string? Validate() => string.IsNullOrEmpty(FileName) ? "Parse Excel Request requiers a file name" : null;
}