using System;
using System.IO;
using System.Threading.Tasks;
using AgentExtraction.Contracts;
using AgentExtraction.Extraction;
using AgentExtraction.Parsing;
using AgentExtraction.Preparation;
using AgentExtraction.RabbitMQ;
using AgentExtraction.Storage;

namespace AgentExtraction;

public class Program
{
    public static async Task Main(string[] args)
    {
        var filePath = ValidateInput(args);
        if (filePath == null)
        {
            return; // Validation failed, exit program
        }

        // Define queue names for the message bus
        var extractionQueueName = "extraction_queue";
        var parsingQueueName = "parsing_queue";
        
        // Define connection string for RabbitMQ
        var connectionString = "amqp://admin:adminpassword@localhost:5672";

        try
        {
            // Create message bus service factory and initialize the service
            var messageBusServiceFactory = new RabbitMQServiceFactory(extractionQueueName, parsingQueueName);
            var messageBusService = await messageBusServiceFactory.CreateAsync(connectionString);
            
            Console.WriteLine("Message bus service initialized successfully.");
            
            // Create storage service
            var storage = new FileSystemStorage();
            
            // Create and subscribe extraction and parsing services
            var extractionService = new ExtractionService(messageBusService, extractionQueueName, parsingQueueName);
            var parsingService = new ParsingService(messageBusService, parsingQueueName);
            
            // Subscribe services to their respective queues
            Console.WriteLine("Setting up extraction service subscription...");
            await extractionService.Subscribe();
            
            Console.WriteLine("Setting up parsing service subscription...");
            await parsingService.Subscribe();
            
            // Create preparation service and start the pipeline
            Console.WriteLine("Initializing preparation service...");
            var preparationService = new PreparationService(messageBusService, storage, extractionQueueName);
            
            // Process document (will publish message to extraction queue internally)
            Console.WriteLine($"Processing document: {filePath}");
            await preparationService.ProcessDocumentAsync(filePath);
            
            Console.WriteLine("Document submitted to the processing pipeline successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during processing: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }
        finally
        {
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    private static string? ValidateInput(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Error: No file path provided.");
            Console.WriteLine("Usage: AgentExtraction <file_path>");
            return null;
        }

        string filePath = args[0];
        
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Error: File not found at path: {filePath}");
            return null;
        }

        string fileExtension = Path.GetExtension(filePath).ToLower();
        if (fileExtension != ".pdf" && fileExtension != ".tiff" && fileExtension != ".tif")
        {
            Console.WriteLine($"Error: Unsupported file format. Only PDF and TIFF formats are supported.");
            return null;
        }

        return filePath;
    }
}
