using commons.Protos;
using System.Text.Json;

namespace commons.EventBase;

public record Envelope
{
    public string EventType { get; set; } = string.Empty;
    public JsonElement Payload { get; set; }
    public Envelope()
    {
    }
    public Envelope(string eventType, MessageBody body)
    {
        EventType = eventType;
        Payload = body.Json;
    }
}