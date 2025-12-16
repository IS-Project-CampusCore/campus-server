namespace MyGrpcService.Implementation;

public class MyGrpcServiceServiceImplementation(
    ILogger<MyGrpcServiceServiceImplementation> logger
)
{
    private readonly ILogger<MyGrpcServiceServiceImplementation> _logger = logger;
}
