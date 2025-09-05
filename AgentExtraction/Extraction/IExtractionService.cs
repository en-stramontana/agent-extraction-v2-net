using System.Threading.Tasks;

namespace AgentExtraction.Extraction;

/// <summary>
/// Interface for the extraction phase of document processing.
/// Handles additional data extraction from processed documents using LLM.
/// </summary>
public interface IExtractionService
{
    /// <summary>
    /// Subscribes to the preparation phase queue and processes documents when messages are received.
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    Task Subscribe();
}
