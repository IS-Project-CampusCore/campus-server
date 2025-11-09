using System.Text.Json;

namespace commons.Protos;

public partial class MessageResponse
{
    public MessageBody GetBody() => StringToMessageBody(Body);

    public static MessageResponse Ok(object? response = null)
    {
        return new MessageResponse
        {
            Success = true,
            Code = 200,
            Body = BodyToString(response),
        };
    }
    public static MessageResponse BadRequest(string errorMessage, object? response = null)
    {
        return new MessageResponse
        {
            Success = false,
            Code = 400,
            Body = BodyToString(response),
            Errors = errorMessage
        };
    }
    public static MessageResponse Unauthorized(string errorMessage, object? response = null)
    {
        return new MessageResponse
        {
            Success = false,
            Code = 401,
            Body = BodyToString(response),
            Errors = errorMessage
        };
    }

    public static MessageResponse Forbidden(string errorMessage, object? response = null)
    {
        return new MessageResponse
        {
            Success = false,
            Code = 403,
            Body = BodyToString(response),
            Errors = errorMessage
        };
    }

    public static MessageResponse NotFound(string errorMessage, object? response = null)
    {
        return new MessageResponse
        {
            Success = false,
            Code = 404,
            Body = BodyToString(response),
            Errors = errorMessage
        };
    }

    public static MessageResponse Error(string errorMessage, object? response = null)
    {
        return new MessageResponse
        {
            Success = false,
            Code = 500,
            Body = BodyToString(response),
            Errors = errorMessage
        };
    }

    public static MessageResponse Error(Exception ex)
    {
        return new MessageResponse
        {
            Success = false,
            Code = 500,
            Body = null,
            Errors = ex.Message
        };
    }

    public static string BodyToString(MessageBody body) => body.Json.ToString();

    public static string BodyToString(object? body) => body is string ? (string)body : JsonSerializer.Serialize(body);

    public static MessageBody StringToMessageBody(string jsonBody) => new(JsonDocument.Parse(jsonBody).RootElement);
}

public readonly struct MessageBody
{
    private static readonly JsonDocument s_nullJson = JsonDocument.Parse("null");

    private readonly JsonElement _json;

    public readonly JsonElement Json => _json;

    public MessageBody() => _json = s_nullJson.RootElement;
    public MessageBody(JsonElement json) => _json = json;

    public readonly string String() => ValidateJsonOrThrow(JsonValueKind.String).ToString();


    private bool Validate(JsonValueKind kind) => _json.ValueKind == kind;
    private JsonElement ValidateJsonOrThrow(JsonValueKind valueKind)
    {
        if (!Validate(valueKind))
            throw new ArgumentException($"Message body is not a valid {valueKind}");    //To be changed to a MessageBodyException or sth

        return _json;
    }
}

