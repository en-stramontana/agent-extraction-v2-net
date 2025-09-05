using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AgentExtraction.RabbitMQ;

/// <summary>
/// Async message consumer for RabbitMQ using the latest API (v7+)
/// </summary>
internal class AsyncMessageConsumer : IAsyncBasicConsumer
{
    private readonly IChannel _channel;
    private readonly Func<ReadOnlyMemory<byte>, IReadOnlyBasicProperties, ulong, CancellationToken, Task> _messageHandler;

    public AsyncMessageConsumer(IChannel channel, Func<ReadOnlyMemory<byte>, IReadOnlyBasicProperties, ulong, CancellationToken, Task> messageHandler)
    {
        _channel = channel;
        _messageHandler = messageHandler;
    }

    // Required IAsyncBasicConsumer properties
    public IChannel Channel => _channel;
    public event AsyncEventHandler<ConsumerEventArgs>? ConsumerCancelled;

    // Required IAsyncBasicConsumer methods
    public Task HandleBasicDeliverAsync(
        string consumerTag,
        ulong deliveryTag, 
        bool redelivered, 
        string exchange, 
        string routingKey, 
        IReadOnlyBasicProperties properties, 
        ReadOnlyMemory<byte> body,
        CancellationToken cancellationToken = default)
    {
        return _messageHandler(body, properties, deliveryTag, cancellationToken);
    }

    public Task HandleBasicCancelAsync(string consumerTag, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Consumer canceled: {consumerTag}");
        ConsumerCancelled?.Invoke(this, new ConsumerEventArgs(new[] { consumerTag }));
        return Task.CompletedTask;
    }

    public Task HandleBasicCancelOkAsync(string consumerTag, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task HandleBasicConsumeOkAsync(string consumerTag, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task HandleChannelShutdownAsync(object channel, ShutdownEventArgs reason)
    {
        Console.WriteLine($"Channel shutdown: {reason.ReplyText}");
        return Task.CompletedTask;
    }

    // Additional methods required by the interface
    public Task HandleBasicRecoverOkAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
