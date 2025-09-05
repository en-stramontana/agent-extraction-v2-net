using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AgentExtraction.Contracts;
using AgentExtraction.Storage;
using Docnet.Core;
using Docnet.Core.Models;
using Newtonsoft.Json;
using Tesseract;

namespace AgentExtraction.Preparation;

/// <summary>
/// Service responsible for the preparation phase of document processing.
/// Handles document pre-processing, conversion to JPG, and OCR processing.
/// </summary>
public class PreparationService : IPreparationService
{
    private readonly IMessageBusService _messageBusService;
    private readonly string _extractionQueueName;
    private readonly IStorage _storage;
    
    public PreparationService(IMessageBusService messageBusService, IStorage storage, string extractionQueueName)
    {
        _messageBusService = messageBusService;
        _storage = storage;
        _extractionQueueName = extractionQueueName;
    }
    
    /// <summary>
    /// Processes a document file, converting it to JPG images and performing OCR.
    /// </summary>
    /// <param name="filePath">Path to the document file (PDF or TIFF)</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task ProcessDocumentAsync(string filePath)
    {
        try
        {
            Console.WriteLine($"[Preparation] Starting preparation for file: {filePath}");
            
            // Generate a correlation ID for this job
            var correlationId = Guid.NewGuid().ToString();
            
            // Store the original file and get its extension
            var fileExtension = await StoreOriginalFile(filePath, correlationId);
            
            // Convert the document to images based on file extension
            // StoreOriginalFile already validated that extension is either pdf or tiff
            var images = fileExtension == "pdf" 
                ? ConvertPdfToImages(filePath) 
                : ConvertTiffToImages(filePath);
            
            // Store the converted images
            await StoreImages(images, correlationId);
            
            // Perform OCR processing on the images
            var ocrResults = ProcessOcr(images);
            
            // Store the OCR results as JSON
            await StoreOcrResults(ocrResults, correlationId);
            
            // Publish a message to the extraction queue
            await _messageBusService.PublishMessageAsync(_extractionQueueName, correlationId);
            Console.WriteLine($"[Preparation] Published message to extraction queue with correlation ID: {correlationId}");
            
            // Clean up the images to free memory
            foreach (var image in images)
            {
                image.Dispose();
            }
            
            Console.WriteLine($"[Preparation] Completed preparation for file: {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Preparation] Error processing document: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Converts a PDF file to an array of System.Drawing.Image objects.
    /// </summary>
    /// <param name="pdfFilePath">Path to the PDF file</param>
    /// <returns>An array of Image objects, one for each page in the PDF</returns>
    private Image[] ConvertPdfToImages(string pdfFilePath)
    {
        if (!File.Exists(pdfFilePath))
        {
            throw new FileNotFoundException($"PDF file not found: {pdfFilePath}");
        }
        
        // Use default page dimensions - A4 at 96 DPI
        var pageDimensions = new PageDimensions(794, 1123);
        
        // The images to return, one per page
        var images = new List<Image>();
        
        // PDFium factory instance
        using var docNetLib = DocLib.Instance;
        
        // Create a document reader with the file path and dimensions
        using var docReader = docNetLib.GetDocReader(pdfFilePath, pageDimensions);
        
        // Process each page in the PDF
        for (var pageIndex = 0; pageIndex < docReader.GetPageCount(); pageIndex++)
        {
            // Get the page reader for the current page
            using var pageReader = docReader.GetPageReader(pageIndex);
            
            // Get the raw BGRA bytes for the page
            var rawBytes = pageReader.GetImage();
            
            // Get the dimensions of the page
            var width = pageReader.GetPageWidth();
            var height = pageReader.GetPageHeight();
            
            // Create a bitmap from the raw bytes
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            
            // Copy the raw bytes to the bitmap
            var rect = new Rectangle(0, 0, width, height);
            var bitmapData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
            
            // Copy the raw bytes to the bitmap
            Marshal.Copy(rawBytes, 0, bitmapData.Scan0, rawBytes.Length);
            bitmap.UnlockBits(bitmapData);
            
            // Add the bitmap to our list
            images.Add(bitmap);
        }
        
        return images.ToArray();
    }
    
    /// <summary>
    /// Converts a TIFF file to an array of System.Drawing.Image objects.
    /// </summary>
    /// <param name="tiffFilePath">Path to the TIFF file</param>
    /// <returns>An array of Image objects, one for each frame in the TIFF</returns>
    private Image[] ConvertTiffToImages(string tiffFilePath)
    {
        if (!File.Exists(tiffFilePath))
        {
            throw new FileNotFoundException($"TIFF file not found: {tiffFilePath}");
        }
        
        // Load the TIFF image
        using var tiffImage = Image.FromFile(tiffFilePath);
        
        // Get the number of frames in the TIFF
        var frameCount = tiffImage.GetFrameCount(FrameDimension.Page);
        
        // List to hold all frames as separate images
        var images = new List<Image>(frameCount);
        
        // Process each frame in the TIFF
        for (var frameIndex = 0; frameIndex < frameCount; frameIndex++)
        {
            // Select the current frame
            tiffImage.SelectActiveFrame(FrameDimension.Page, frameIndex);
            
            // Create a new bitmap from the current frame
            var frameBitmap = new Bitmap(tiffImage.Width, tiffImage.Height);
            
            // Create a graphics object to draw the frame onto the bitmap
            using var graphics = Graphics.FromImage(frameBitmap);
            
            // Draw the current frame onto the bitmap
            graphics.DrawImage(tiffImage, new Rectangle(0, 0, frameBitmap.Width, frameBitmap.Height));
            
            // Add the bitmap to our list of images
            images.Add(frameBitmap);
        }
        
        return images.ToArray();
    }
    
    /// <summary>
    /// Stores the original document file with the proper naming convention
    /// </summary>
    /// <param name="filePath">Path to the original file</param>
    /// <param name="correlationId">Correlation ID for the job</param>
    /// <returns>The extension of the stored file (pdf or tiff)</returns>
    private async Task<string> StoreOriginalFile(string filePath, string correlationId)
    {
        // Get file extension (pdf or tiff)
        var extension = Path.GetExtension(filePath).ToLower();
        
        // Remove the dot from the extension
        if (extension.StartsWith("."))
        {
            extension = extension.Substring(1);
        }
        
        // Validate file extension
        if (extension != "pdf" && extension != "tiff")
        {
            throw new ArgumentException($"Unsupported file extension: {extension}. Only PDF and TIFF files are supported.");
        }
        
        // Create the storage filename using the required naming convention
        var storageName = $"{correlationId}_preparation_original.{extension}";
        
        // Read the original file
        var fileBytes = await File.ReadAllBytesAsync(filePath);
        
        // Store the file
        await _storage.Save(storageName, fileBytes);
        
        Console.WriteLine($"[Preparation] Stored original file as {storageName}");
        
        return extension;
    }
    
    /// <summary>
    /// Stores the converted images with the proper naming convention
    /// </summary>
    /// <param name="images">Array of images to store</param>
    /// <param name="correlationId">Correlation ID for the job</param>
    /// <returns>A task representing the asynchronous operation</returns>
    private async Task StoreImages(Image[] images, string correlationId)
    {
        // Validate input
        if (images == null || images.Length == 0)
        {
            throw new ArgumentException("No images provided to store.");
        }
        
        Console.WriteLine($"[Preparation] Storing {images.Length} converted images");
        
        // Store each image with sequential numbering
        for (var i = 0; i < images.Length; i++)
        {
            // 1-based numbering for the image files
            var pageNumber = i + 1;
            
            // Create the storage filename using the required naming convention
            var storageName = $"{correlationId}_preparation_{pageNumber}.png";
            
            // Convert the image to a PNG byte array
            using var memoryStream = new MemoryStream();
            images[i].Save(memoryStream, ImageFormat.Png);
            var imageBytes = memoryStream.ToArray();
            
            // Store the image
            await _storage.Save(storageName, imageBytes);
            
            Console.WriteLine($"[Preparation] Stored image {pageNumber} as {storageName}");
        }
    }
    
    /// <summary>
    /// Performs OCR on the provided images and returns the OCR results
    /// </summary>
    /// <param name="images">Array of images to process with OCR</param>
    /// <returns>OCR results containing text and coordinates</returns>
    private OcrResult ProcessOcr(Image[] images)
    {
        Console.WriteLine($"[Preparation] Starting OCR processing on {images.Length} images");
        
        // Initialize the Tesseract engine with the English language data
        var tessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
        
        // Create the OCR result object to store all data
        var ocrResult = new OcrResult
        {
            Pages = new List<OcrPage>()
        };
        
        // Process each image with OCR
        for (var i = 0; i < images.Length; i++)
        {
            // 1-based numbering for the page numbers
            var pageNumber = i + 1;
            Console.WriteLine($"[Preparation] Processing OCR for page {pageNumber}");
            
            var ocrPage = new OcrPage
            {
                PageNumber = pageNumber,
                Blocks = new List<OcrBlock>()
            };
            
            // Use Tesseract to process the image
            using var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default);
            
            // Copy the image to a temporary bitmap to avoid modifying the original
            using var bitmap = new Bitmap(images[i]);
            using var page = engine.Process(bitmap);
            
            // Get the text blocks from the page
            using var iterator = page.GetIterator();
            
            // Configure the iterator to get block-level results
            iterator.Begin();
            
            do
            {
                // Process each text block in the page
                if (iterator.TryGetBoundingBox(PageIteratorLevel.Block, out var rect))
                {
                    var text = iterator.GetText(PageIteratorLevel.Block);
                    
                    // Skip empty blocks
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        var confidence = iterator.GetConfidence(PageIteratorLevel.Block);
                        
                        // Add the block to our results
                        ocrPage.Blocks.Add(new OcrBlock
                        {
                            Text = text.Trim(),
                            X = rect.X1,
                            Y = rect.Y1,
                            Width = rect.Width,
                            Height = rect.Height,
                            Confidence = confidence
                        });
                    }
                }
            }
            while (iterator.Next(PageIteratorLevel.Block));
            
            // Add the page to our results if it has blocks
            if (ocrPage.Blocks.Count > 0)
            {
                ocrResult.Pages.Add(ocrPage);
            }
        }
        
        Console.WriteLine($"[Preparation] Completed OCR processing. Found {ocrResult.Pages.Count} pages with text.");
        return ocrResult;
    }
    
    /// <summary>
    /// Stores the OCR results as a JSON file
    /// </summary>
    /// <param name="ocrResult">The OCR results to store</param>
    /// <param name="correlationId">Correlation ID for the job</param>
    /// <returns>A task representing the asynchronous operation</returns>
    private async Task StoreOcrResults(OcrResult ocrResult, string correlationId)
    {
        // Create the storage filename using the required naming convention
        var storageName = $"{correlationId}_preparation_ocr.json";
        
        // Serialize the OCR results to JSON
        var jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };
        
        var jsonContent = JsonConvert.SerializeObject(ocrResult, jsonSettings);
        var contentBytes = System.Text.Encoding.UTF8.GetBytes(jsonContent);
        
        // Store the JSON file
        await _storage.Save(storageName, contentBytes);
        
        Console.WriteLine($"[Preparation] Stored OCR results as {storageName}");
    }
}
