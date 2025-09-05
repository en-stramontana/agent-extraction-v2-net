using System;
using System.Threading.Tasks;

namespace AgentExtraction.Contracts;

/// <summary>
/// Interface for messaging services that can publish and consume messages.
/// Provides an abstraction over the underlying message bus implementation.
/// </summary>
public interface IMessageBusService
{
    /// <summary>
    /// Publishes a message to the specified queue.
    /// </summary>
    /// <param name="queueName">Name of the queue to publish to</param>
    /// <param name="correlationId">Correlation ID for tracking the message</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task PublishMessageAsync(string queueName, string correlationId);
    
    /// <summary>
    /// Consumes messages from the specified queue and processes them using the provided callback.
    /// </summary>
    /// <param name="queueName">Name of the queue to consume from</param>
    /// <param name="processMessageCallback">Callback function to process each message, providing only the correlation ID</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task ConsumeMessagesAsync(string queueName, Func<string, Task> processMessageCallback);
}
