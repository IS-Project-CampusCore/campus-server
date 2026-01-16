using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZstdSharp.Unsafe;

namespace commons.Protos;

public partial class MessageResponse
{
    [JsonIgnore]
    public MessageBody Payload
    {
        get
        {
            if (string.IsNullOrEmpty(Body))
                return new MessageBody();

            return StringToMessageBody(Body);
        }
        set
        {
            Body = value.Json.ValueKind == JsonValueKind.Undefined ? string.Empty : value.Json.ToString();
        }
    }

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
    public T? GetPayload<T>(JsonSerializerOptions? options = null)
    {
        return Payload.Json.Deserialize<T>(options);
    }

    public static string BodyToString(object? body)
    {
        if (body is null) 
            return string.Empty;

        if (body is MessageBody mb) 
            return mb.Json.ToString();

        return JsonSerializer.Serialize(body);
    }

    public static MessageBody StringToMessageBody(string jsonBody)
    {
        if (string.IsNullOrWhiteSpace(jsonBody)) 
            return new MessageBody();

        return new MessageBody(JsonDocument.Parse(jsonBody).RootElement);
    }
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

    public readonly MessageBody Array() => new(ValidateJsonOrThrow(JsonValueKind.Array));

    public readonly bool Bool()
    {
        bool? val = TryBool();
        if (val is not null)
            return val.Value;

        throw new ArgumentException($"Message body is not a valid bool");
    }

    public readonly string? TryString() => Validate(JsonValueKind.String) ? _json.GetString() : null;
   
    public readonly int? TryInt32() => Validate(JsonValueKind.Number) ? _json.GetInt32() : null;

    public readonly MessageBody? TryObj() => Validate(JsonValueKind.Object) ? new(_json) : null;

    public readonly MessageBody? TryArray() => Validate(JsonValueKind.Array) ? new(_json) : null;

    public readonly bool? TryBool()
    {
        if (Validate(JsonValueKind.True))
            return true;
        else if (Validate(JsonValueKind.False))
            return false;

        return null;
    }

    public readonly string? TryGetString(string property) =>
        TryGetProperty(property, JsonValueKind.String) is { } prop ? prop.GetString() : null;

    public readonly int? TryGetInt32(string property) =>
        TryGetProperty(property, JsonValueKind.Number) is { } prop ? prop.GetInt32() : null;

    public readonly MessageBody? TryGetObject(string property) => 
        TryGetProperty(property, JsonValueKind.Object) is { } prop ? new(prop) : null;

    public readonly MessageBody? TryGetArray(string property) => 
        TryGetProperty(property, JsonValueKind.Array) is { } prop ? new(prop) : null;

    public readonly bool? TryGetBool(string property)
    {
        bool? val = TryGetProperty(property, JsonValueKind.True) is { } prop1 ? prop1.GetBoolean() : null;
        if (val is not null)
            return val.Value;

        val = TryGetProperty(property, JsonValueKind.False) is { } prop2 ? prop2.GetBoolean() : null;
        if (val is not null)
            return val.Value;

        return null;
    }

    public readonly string GetString(string property) =>
        GetPropertyAndValidate(property, JsonValueKind.String).GetString()!;

    public readonly int GetInt32(string property) =>
        GetPropertyAndValidate(property, JsonValueKind.Number).GetInt32();

    public readonly MessageBody GetObject(string property) =>
    new(GetPropertyAndValidate(property, JsonValueKind.Object));

    public readonly MessageBody GetArray(string property) =>
        new(GetPropertyAndValidate(property, JsonValueKind.Array));

    public readonly bool GetBool(string property)
    {
        if (!TryGetProperty(property).HasValue)
        {
            throw new ArgumentException($"Property '{property}' not found on message body object.");
        }

        bool? val = TryGetBool(property);
        if (val is not null)
            return val.Value;

        throw new ArgumentException($"Property '{property}' is not a valid Bool.");
    }

    public readonly IEnumerable<MessageBody> Iterate()
    {
        if (!Validate(JsonValueKind.Array))
            throw new ArgumentException("Can not iterate message body. Expected array");

        return _json.EnumerateArray().Select(json => new MessageBody(json));
    }
    public readonly IEnumerable<string> IterateStrings() => Iterate().Select(e => e.String());

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