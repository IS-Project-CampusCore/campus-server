using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace commons;

public class MessageResponse
{
    public bool Success { get; set; }
    public int Code { get; set; }
    public object? Body { get; set; }
    public string? Errors { get; set; }
}

public readonly struct MessageBody
{
    private static readonly JsonDocument s_nullJson = JsonDocument.Parse("null");

    private readonly JsonElement _json;

    public readonly JsonElement Json => _json;

    public MessageBody() => _json = s_nullJson.RootElement;
    public MessageBody(JsonElement json) => _json = json;

    public readonly string String() => ValidateJsonOrThrow(JsonValueKind.String).ToString();
    public readonly int Int32() => ValidateJsonOrThrow(JsonValueKind.Number).GetInt32();
    public readonly JsonElement Object() => ValidateJsonOrThrow(JsonValueKind.Object);
    public readonly JsonElement.ArrayEnumerator Array() => ValidateJsonOrThrow(JsonValueKind.Array).EnumerateArray();

    public readonly string? TryString() => Validate(JsonValueKind.String) ? _json.GetString() : null;
    public readonly int? TryInt32() => Validate(JsonValueKind.Number) ? _json.GetInt32() : null;
    public readonly JsonElement? TryObj() => Validate(JsonValueKind.Object) ? _json : null;
    public readonly JsonElement.ArrayEnumerator? TryArray() => Validate(JsonValueKind.Array) ? _json.EnumerateArray() : null;

    private bool Validate(JsonValueKind kind) => _json.ValueKind == kind;
    private JsonElement ValidateJsonOrThrow(JsonValueKind valueKind)
    {
        if (!Validate(valueKind))
            throw new ArgumentException($"Message body is not a valid {valueKind}");    //To be changed to a MessageBodyException or sth

        return _json;
    }
}