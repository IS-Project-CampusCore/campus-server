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
            Body = "",
            Errors = ex.Message
        };
    }

    public static string BodyToString(MessageBody body) => body.Json.ToString();

    public static string BodyToString(object? body) => BodyToString(MessageBody.From(body));

    public static MessageBody StringToMessageBody(string jsonBody) => new(JsonDocument.Parse(jsonBody).RootElement);
}

public readonly struct MessageBody
{
    private static readonly JsonDocument s_nullJson = JsonDocument.Parse("null");

    private readonly JsonElement _json;

    public readonly JsonElement Json => _json;

    public MessageBody() => _json = s_nullJson.RootElement;
    
    public MessageBody(JsonElement json) => _json = json;

    public static MessageBody From<T>(T value, JsonSerializerOptions? options = null)
    {
        JsonElement element = JsonSerializer.SerializeToElement(value, options);
        return new MessageBody(element);
    }

    public override string ToString()
    {
        return _json.ValueKind switch
        {
            JsonValueKind.String => _json.GetString() ?? string.Empty,

            JsonValueKind.Null => string.Empty,

            _ => _json.GetRawText()
        };
    }

    public readonly string String() => ValidateJsonOrThrow(JsonValueKind.String).GetString()!;
    
    public readonly int Int32() => ValidateJsonOrThrow(JsonValueKind.Number).GetInt32();

    public readonly MessageBody Object() => new(ValidateJsonOrThrow(JsonValueKind.Object));

    public readonly JsonElement Array() => ValidateJsonOrThrow(JsonValueKind.Array);

    public readonly string? TryString() => Validate(JsonValueKind.String) ? _json.GetString() : null;
   
    public readonly int? TryInt32() => Validate(JsonValueKind.Number) ? _json.GetInt32() : null;

    public readonly MessageBody? TryObj() => Validate(JsonValueKind.Object) ? new(_json) : null;

    public readonly JsonElement? TryArray() => Validate(JsonValueKind.Array) ? _json : null;

    public readonly string? TryGetString(string property) =>
        TryGetProperty(property, JsonValueKind.String) is { } prop ? prop.GetString() : null;

    public readonly int? TryGetInt32(string property) =>
        TryGetProperty(property, JsonValueKind.Number) is { } prop ? prop.GetInt32() : null;

    public readonly MessageBody? TryGetObject(string property) => 
        TryGetProperty(property, JsonValueKind.Object) is { } prop ? new(prop) : null;

    public readonly JsonElement? TryGetArray(string property) => 
        TryGetProperty(property, JsonValueKind.Array) is { } prop ? prop : null;

    public readonly string GetString(string property) =>
        GetPropertyAndValidate(property, JsonValueKind.String).GetString()!;

    public readonly int GetInt32(string property) =>
        GetPropertyAndValidate(property, JsonValueKind.Number).GetInt32();

    public readonly MessageBody GetObject(string property) =>
    new(GetPropertyAndValidate(property, JsonValueKind.Object));

    public readonly JsonElement GetArray(string property) =>
        GetPropertyAndValidate(property, JsonValueKind.Array);

    private readonly JsonElement GetPropertyAndValidate(string propertyName, JsonValueKind expectedKind)
    {
        JsonElement? propertyElement = TryGetProperty(propertyName);

        if (!propertyElement.HasValue)
        {
            throw new ArgumentException($"Property '{propertyName}' not found on message body object."); 
        }

        if (!ValidateProperty(propertyElement.Value, expectedKind))
        {
            throw new ArgumentException($"Property '{propertyName}' is not a valid {expectedKind}."); 
        }

        return propertyElement.Value;
    }

    private readonly JsonElement? TryGetProperty(string propertyName) =>
     _json.ValueKind == JsonValueKind.Object && _json.TryGetProperty(propertyName, out var element)
         ? element
         : null;

    private readonly JsonElement? TryGetProperty(string propertyName, JsonValueKind expectedKind)
    {
        var element = TryGetProperty(propertyName);
        return (element.HasValue && ValidateProperty(element.Value, expectedKind))
            ? element.Value
            : null;
    }

    private readonly bool ValidateProperty(JsonElement element, JsonValueKind expectedKind) =>
        element.ValueKind == expectedKind;


    private bool Validate(JsonValueKind kind) => _json.ValueKind == kind;

    private JsonElement ValidateJsonOrThrow(JsonValueKind valueKind)
    {
        if (!Validate(valueKind))
            throw new ArgumentException($"Message body is not a valid {valueKind}");    //To be changed to a MessageBodyException or sth

        return _json;
    }
}