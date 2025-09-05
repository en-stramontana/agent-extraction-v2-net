using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
using AgentExtraction.Contracts;

namespace AgentExtraction.RabbitMQ;

public class RabbitMQService : IMessageBusService
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    public RabbitMQService(IConnection connection, IChannel channel)
    {
        _connection = connection;
        _channel = channel;
    }

    public async Task PublishMessageAsync(string queueName, string correlationId)
    {
        try
        {
            var properties = new BasicProperties
            {
                Persistent = true,
                CorrelationId = correlationId
            };
            
            // Use empty array as message body as requested
            var emptyBody = Array.Empty<byte>();
            
            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: queueName,
                mandatory: false,
                basicProperties: properties,
                body: emptyBody);
            
            Console.WriteLine($"Message published to {queueName} with correlation ID: {correlationId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error publishing message to {queueName}: {ex.Message}");
            throw;
        }
    }

    public async Task ConsumeMessagesAsync(string queueName, Func<string, Task> processMessageCallback)
    {
        // Create an async consumer for the latest RabbitMQ API (v7+)
        var consumer = new AsyncMessageConsumer(_channel, async (body, properties, deliveryTag, cancellationToken) =>
        {
            try
            {
                var correlationId = properties.CorrelationId ?? string.Empty;
                
                Console.WriteLine($"Processing message from {queueName} with correlation ID: {correlationId}");
                
                // Pass only the correlationId to the callback
                await processMessageCallback(correlationId);
                
                // Acknowledge the message
                await _channel.BasicAckAsync(deliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message from {queueName}: {ex.Message}");
                // Reject the message and requeue
                await _channel.BasicNackAsync(deliveryTag, multiple: false, requeue: true);
            }
        });
        
        // Register the consumer using the proper API in v7+
        string consumerTag = await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumerTag: "",
            noLocal: false,
            exclusive: false,
            arguments: null,
            consumer: consumer);
        
        Console.WriteLine($"Consumer registered for queue: {queueName} with tag: {consumerTag}");
    }
}
