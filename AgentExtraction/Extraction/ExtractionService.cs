using System;
using System.Threading.Tasks;
using AgentExtraction.Contracts;

namespace AgentExtraction.Extraction;

/// <summary>
/// Service responsible for the extraction phase of document processing.
/// Handles additional data extraction from processed documents using LLM.
/// </summary>
public class ExtractionService : IExtractionService
{
    private readonly IMessageBusService _messageBusService;
    private readonly string _parsingQueueName;
    private readonly string _extractionQueueName;
    
    public ExtractionService(IMessageBusService messageBusService, string extractionQueueName, string parsingQueueName)
    {
        _messageBusService = messageBusService;
        _extractionQueueName = extractionQueueName;
        _parsingQueueName = parsingQueueName;
    }
    
    /// <summary>
    /// Subscribes to the preparation phase queue and processes documents when messages are received.
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task Subscribe()
    {
        try
        {
            Console.WriteLine("[Extraction] Starting extraction process");
            
            // Subscribe to the message bus with the ProcessMessageAsync method
            await _messageBusService.ConsumeMessagesAsync(_extractionQueueName, ProcessMessageAsync);
            
            Console.WriteLine("[Extraction] Completed extraction process");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Extraction] Error extracting data: {ex.Message}");
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
        Console.WriteLine($"[Extraction] Processing message with correlation ID: {correlationId}");
        
        // This method only contains a console message as requested
        // Additional processing logic can be added here in the future
        
        // Publish message to parsing queue with the same correlation ID
        Console.WriteLine($"[Extraction] Publishing message to parsing queue: {_parsingQueueName}");
        await _messageBusService.PublishMessageAsync(_parsingQueueName, correlationId);
    }
}
