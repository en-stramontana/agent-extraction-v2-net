using System;
using System.Threading.Tasks;
using AgentExtraction.Contracts;

namespace AgentExtraction.Parsing;

/// <summary>
/// Service responsible for the parsing phase of document processing.
/// Handles structuring and formatting the extracted data for target systems.
/// </summary>
public class ParsingService : IParsingService
{
    private readonly IMessageBusService _messageBusService;
    private readonly string _parsingQueueName;
    
    public ParsingService(IMessageBusService messageBusService, string parsingQueueName)
    {
        _messageBusService = messageBusService;
        _parsingQueueName = parsingQueueName;
    }
    
    /// <summary>
    /// Subscribes to the extraction phase queue and processes data when messages are received.
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task Subscribe()
    {
        try
        {
            Console.WriteLine("[Parsing] Starting parsing process");
            
            // Subscribe to the message bus with the ProcessMessageAsync method
            await _messageBusService.ConsumeMessagesAsync(_parsingQueueName, ProcessMessageAsync);
            
            Console.WriteLine("[Parsing] Completed parsing process");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Parsing] Error parsing data: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Processes messages received from the message bus.
    /// </summary>
    /// <param name="correlationId">The correlation ID of the message</param>
    /// <returns>A task representing the asynchronous operation</returns>
    private async Task ProcessMessageAsync(string correlationId)
    {
        Console.WriteLine($"[Parsing] Processing message with correlation ID: {correlationId}");
        
        // This method only contains a console message as requested
        // Additional processing logic can be added here in the future
        
        await Task.CompletedTask;
    }
}
