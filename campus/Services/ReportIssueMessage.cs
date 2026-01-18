using campus.Models;
using campus.Implementation;
using campusServiceClient;
using commons.RequestBase;

namespace campus.Services;

public class ReportIssueMessage(
    ILogger<ReportIssueMessage> logger,
    CampusServiceImplementation implementation
) : CampusMessage<ReportIssueRequest,Issue>(logger)
{
    private readonly CampusServiceImplementation _impl = implementation;

    protected override async Task<Issue> HandleMessage(ReportIssueRequest request, CancellationToken token) 
        => await _impl.ReportIssueAsync(request.IssuerId,request.Location,request.Description);
}
