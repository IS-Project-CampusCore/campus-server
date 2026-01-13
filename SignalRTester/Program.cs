using commons.SignalRBase;
using Microsoft.AspNetCore.SignalR.Client;
using System.Reflection;
using System.Text.Json;

Console.Title = "Campus SignalR Tester (Internal Tool)";
var hubUrl = "http://10.172.46.35:8081/hubs/chat";

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("╔════════════════════════════════════════════════════╗");
Console.WriteLine("║           CAMPUS SIGNALR TESTER (v1.0)             ║");
Console.WriteLine("╚════════════════════════════════════════════════════╝");
Console.ResetColor();

Console.Write("Enter JWT Access Token: ");
var token = Console.ReadLine();

if (string.IsNullOrWhiteSpace(token))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Token is required to connect.");
    Console.ResetColor();
    return;
}

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("\nScanning 'notification' assembly for event definitions...");
Console.ResetColor();

var notificationAssembly = Assembly.Load("notification");
var definitions = notificationAssembly.GetTypes()
    .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Contains(typeof(ISignalRDefinition)));

var discoveredEvents = new List<string>();

var connection = new HubConnectionBuilder()
    .WithUrl(hubUrl, options =>
    {
        options.AccessTokenProvider = () => Task.FromResult<string?>(token);
    })
    .WithAutomaticReconnect()
    .Build();

foreach (var type in definitions)
{
    var message = (string)type.GetProperty("Message")!.GetValue(null)!;
    var content = type.GetProperty("Content")!.GetValue(null);

    discoveredEvents.Add(message);

    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine($"[Found] {message}");
    Console.ResetColor();

    connection.On<object>(message, (payload) =>
    {
        PrintEvent(message, payload, content);
    });
}

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"Registered listeners for {discoveredEvents.Count} event types.\n");
Console.ResetColor();

connection.Closed += async (error) =>
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[SYSTEM] Disconnected: {error?.Message}");
    Console.ResetColor();
    await Task.CompletedTask;
};

connection.Reconnected += async (connectionId) =>
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"[SYSTEM] Reconnected! New ID: {connectionId}");
    Console.ResetColor();
    await Task.CompletedTask;
};

try
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("Connecting to Hub...");
    Console.ResetColor();

    await connection.StartAsync();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($" Connected! (ID: {connection.ConnectionId})");
    Console.ResetColor();

    Console.WriteLine("Listening for events... (Press Ctrl+C to quit)");
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\nConnection Failed: {ex.Message}");
    Console.ResetColor();
    return;
}

await Task.Delay(-1);

static void PrintEvent(string eventName, object actualPayload, object? expectedSchema)
{
    var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    var jsonString = JsonSerializer.Serialize(actualPayload, jsonOptions);

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.WriteLine($"EVENT RECEIVED: [{eventName}]");
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine(jsonString);

    Console.ResetColor();
    Console.WriteLine();
}