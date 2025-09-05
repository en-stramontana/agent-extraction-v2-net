using System.Threading.Tasks;

namespace AgentExtraction.Preparation;

/// <summary>
/// Interface for the preparation phase of document processing.
/// Handles document pre-processing, conversion to JPG, and OCR processing.
/// </summary>
public interface IPreparationService
{
    /// <summary>
    /// Processes a document file, converting it to JPG images and performing OCR.
    /// </summary>
    /// <param name="filePath">Path to the document file (PDF or TIFF)</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task ProcessDocumentAsync(string filePath);
}
