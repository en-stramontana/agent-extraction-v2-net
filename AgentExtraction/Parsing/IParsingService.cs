using System.Threading.Tasks;

namespace AgentExtraction.Parsing;

/// <summary>
/// Interface for the parsing phase of document processing.
/// Handles structuring and formatting the extracted data for target systems.
/// </summary>
public interface IParsingService
{
    /// <summary>
    /// Subscribes to the extraction phase queue and processes data when messages are received.
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    Task Subscribe();
}
