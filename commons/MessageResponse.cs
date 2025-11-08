using commons.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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

    public static MessageResponse FromProtoMessage(ProtoMessageResponse messageResponse)
    {
        return new MessageResponse
        {
            Success = messageResponse.Success,
            Code = messageResponse.Code,
            Body = messageResponse.Body,
            Errors = messageResponse.Errors,
        };
    }

    public static MessageResponse Ok (object? response = null)
    {
        return new MessageResponse
        {
            Success = true,
            Code = 200,
            Body = response,
            Errors = null
        };
    }
    public static MessageResponse BadRequest(string errorMessage, object? response = null)
    {
        return new MessageResponse
        {
            Success = false,
            Code = 400,
            Body = response,
            Errors = errorMessage
        };
    }
    public static MessageResponse Unauthorized(string errorMessage, object? response = null) {
            return new MessageResponse
            {
                Success = false,
                Code = 401,
                Body = response,
                Errors = errorMessage
            };
        }
    public static MessageResponse Forbidden(string errorMessage, object? response = null) {
        return new MessageResponse
        {
            Success = false,
            Code = 403,
            Body = response,
            Errors = errorMessage
        };
    }

    public static MessageResponse NotFound(string errorMessage, object? response = null)
    {
        return new MessageResponse
        {
            Success = false,
            Code = 404,
            Body = response,
            Errors = errorMessage
        };
    }

    public static MessageResponse Error(string errorMessage, object? response = null)
    {
        return new MessageResponse
        {
            Success = false,
            Code = 500,
            Body = response,
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

