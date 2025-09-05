using System;
using System.Threading.Tasks;
using AgentExtraction.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AgentExtraction.RabbitMQ;

public class RabbitMQServiceFactory : IMessageBusServiceFactory, IAsyncDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;
    private bool _disposed = false;
    private readonly string _extractionQueueName;
    private readonly string _parsingQueueName;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMQServiceFactory"/> class.
    /// </summary>
    /// <param name="extractionQueueName">The name of the extraction queue.</param>
    /// <param name="parsingQueueName">The name of the parsing queue.</param>
    public RabbitMQServiceFactory(string extractionQueueName, string parsingQueueName)
    {
        _extractionQueueName = extractionQueueName;
        _parsingQueueName = parsingQueueName;
    }

    public async Task<IMessageBusService> CreateAsync(string connectionString)
    {
        try
        {
            // Only create new connections if the factory was disposed or connections don't exist
            if (_disposed || _connection == null || _channel == null)
            {
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(connectionString)
                };

                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();
                
                if (_connection == null || _channel == null)
                {
                    throw new InvalidOperationException("Failed to create RabbitMQ connection or channel");
                }
                
                // Log successful connection
                Console.WriteLine("Successfully connected to RabbitMQ server");
                
                // Declare necessary queues before returning the service
                await DeclareQueuesAsync(_channel);
                
                // Reset the disposed flag since we have new connections
                _disposed = false;
            }
            
            return new RabbitMQService(_connection, _channel);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize RabbitMQ: {ex.Message}");
            throw;
        }
    }

    private async Task DeclareQueuesAsync(IChannel channel)
    {
        if (channel == null)
            throw new ArgumentNullException(nameof(channel), "Channel cannot be null");
            
        // Declare queues for each pipeline stage
        await channel.QueueDeclareAsync(_extractionQueueName, durable: true, exclusive: false, autoDelete: false);
        await channel.QueueDeclareAsync(_parsingQueueName, durable: true, exclusive: false, autoDelete: false);
        
        Console.WriteLine("RabbitMQ queues declared successfully");
    }
    
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_disposed)
            return;

        // Dispose managed resources asynchronously
        if (_channel != null)
        {
            await _channel.CloseAsync();
            await _channel.DisposeAsync();
            _channel = null;
        }

        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }

        _disposed = true;
    }
}
