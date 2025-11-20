using commons.Protos;
using MediatR;

namespace excelServiceClient;

public partial class InsertExcelRequest : IRequest<MessageResponse>;

public partial class UpdateExcelRequest : IRequest<MessageResponse>;

public partial class UpsertExcelRequest : IRequest<MessageResponse>;

public partial class ParseExcelRequest : IRequest<MessageResponse>;



