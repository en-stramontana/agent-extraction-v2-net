using System.Threading.Tasks;

namespace AgentExtraction.Contracts;

/// <summary>
/// Factory interface for creating message bus service instances.
/// Provides an abstraction over specific message bus implementation factories.
/// </summary>
public interface IMessageBusServiceFactory
{
    /// <summary>
    /// Creates and initializes a message bus service asynchronously.
    /// </summary>
    /// <param name="connectionString">Connection string for the message bus service</param>
    /// <returns>A task that resolves to an initialized IMessageBusService instance</returns>
    Task<IMessageBusService> CreateAsync(string connectionString);
}
