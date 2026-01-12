using MassTransit;
using RabbitMQ.Client;
using System.Reflection;
using System.Text.RegularExpressions;

namespace commons.EventBase;

public static class MassTransitExtension
{
    public static void RegisterConsumers(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context, Assembly assembly, string queuePrefix)
    {
        var consumers = assembly.GetTypes()
            .Where(t => !t.IsAbstract && typeof(IConsumer<Envelope>).IsAssignableFrom(t))
            .Select(t => new
            {
                Type = t,
                Attribute = t.GetCustomAttribute<EnvelopeAttribute>()
            })
            .Where(x => x.Attribute is not null);

        foreach (var consumer in consumers)
        {
            var eventName = consumer.Attribute!.EventName;

            var queueName = $"{queuePrefix}-{PascalToKebabCase(eventName)}";

            cfg.ReceiveEndpoint(queueName, e =>
            {
                e.ConfigureConsumeTopology = false;

                e.Bind("commons.EventBase:Envelope", s =>
                {
                    s.RoutingKey = eventName;
                    s.ExchangeType = ExchangeType.Direct;
                });

                e.ConfigureConsumer(context, consumer.Type);
            });
        }
    }

    private static string PascalToKebabCase(string value)
    {
        if (string.IsNullOrEmpty(value)) 
            return value;
        return Regex.Replace(value, "(?<!^)([A-Z])", "-$1", RegexOptions.Compiled)
            .Trim().ToLower();
    }
}
