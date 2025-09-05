using System.Collections.Generic;
using Newtonsoft.Json;

namespace AgentExtraction.Preparation;

/// <summary>
/// Represents the OCR results for a document with multiple pages.
/// </summary>
public class OcrResult
{
    /// <summary>
    /// List of pages with OCR data.
    /// </summary>
    [JsonProperty("pages")]
    public List<OcrPage> Pages { get; set; } = new();
}

/// <summary>
/// Represents OCR data for a single page.
/// </summary>
public class OcrPage
{
    /// <summary>
    /// Page number (1-based).
    /// </summary>
    [JsonProperty("pageNumber")]
    public int PageNumber { get; set; }

    /// <summary>
    /// List of text blocks found on the page.
    /// </summary>
    [JsonProperty("blocks")]
    public List<OcrBlock> Blocks { get; set; } = new();
}

/// <summary>
/// Represents a block of text with its coordinates.
/// </summary>
public class OcrBlock
{
    /// <summary>
    /// The recognized text.
    /// </summary>
    [JsonProperty("text")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// X-coordinate of the top-left corner.
    /// </summary>
    [JsonProperty("x")]
    public int X { get; set; }

    /// <summary>
    /// Y-coordinate of the top-left corner.
    /// </summary>
    [JsonProperty("y")]
    public int Y { get; set; }

    /// <summary>
    /// Width of the text block.
    /// </summary>
    [JsonProperty("width")]
    public int Width { get; set; }

    /// <summary>
    /// Height of the text block.
    /// </summary>
    [JsonProperty("height")]
    public int Height { get; set; }

    /// <summary>
    /// Confidence level of the OCR result (0-100).
    /// </summary>
    [JsonProperty("confidence")]
    public float Confidence { get; set; }
}
