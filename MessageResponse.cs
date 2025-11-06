using System;
using System.Text.Json;

public class MessageResponse
{
    public bool Success { get; set; }
    public int Code { get; set; };
    public object? Body { get; set; };
    public string? Errors { get; set; }
}

public readonly struct MessageBody
{
    private readonly JsonElement _json;

    public readonly JsonElement Json => _json;

    private static ValidateOrThrow()
}
