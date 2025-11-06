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
    public readonly string? TryGetString(string property) =>
        TryGetProperty(property, JsonValueKind.String, out var prop) ? prop.GetString() : null;

    public readonly int? TryGetInt32(string property) =>
        TryGetProperty(property, JsonValueKind.Number, out var prop) ? prop.GetInt32() : null;

    public readonly JsonElement? TryGetObject(string property) =>
        TryGetProperty(property, JsonValueKind.Object, out var prop) ? prop : null;

    public readonly JsonElement.ArrayEnumerator? TryGetArray(string property) =>
        TryGetProperty(property, JsonValueKind.Array, out var prop) ? prop.EnumerateArray() : null;

    public readonly string GetString(string property) =>
        GetPropertyAndValidate(property, JsonValueKind.String).GetString()!;

    public readonly int GetInt32(string property) =>
        GetPropertyAndValidate(property, JsonValueKind.Number).GetInt32();

    public readonly JsonElement GetObject(string property) =>
        GetPropertyAndValidate(property, JsonValueKind.Object);

    public readonly JsonElement.ArrayEnumerator GetArray(string property) =>
        GetPropertyAndValidate(property, JsonValueKind.Array).EnumerateArray();

    private readonly JsonElement GetPropertyAndValidate(string propertyName, JsonValueKind expectedKind)
    {
        ValidateJsonOrThrow(JsonValueKind.Object);

        if (!_json.TryGetProperty(propertyName, out var propertyElement))
        {
            throw new ArgumentException($"Property '{propertyName}' not found on message body object."); // Or future MessageBodyException
        }

        if (propertyElement.ValueKind != expectedKind)
        {
            throw new ArgumentException($"Property '{propertyName}' is not a valid {expectedKind}."); // Or future MessageBodyException
        }

        return propertyElement;
    }

    private readonly bool TryGetProperty(string propertyName, out JsonElement element)
    {
        if (_json.ValueKind == JsonValueKind.Object)
        {
            return _json.TryGetProperty(propertyName, out element);
        }
        element = default;
        return false;
    }

    private readonly bool TryGetProperty(string propertyName, JsonValueKind expectedKind, out JsonElement element) =>
        TryGetProperty(propertyName, out element) && ValidateProperty(element, expectedKind);


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