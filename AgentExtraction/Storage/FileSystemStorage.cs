using System.IO;
using System.Reflection;

namespace AgentExtraction.Storage;

/// <summary>
/// Implementation of IStorage using the local file system
/// </summary>
public class FileSystemStorage : IStorage
{
    /// <summary>
    /// Reads a file from the file system
    /// </summary>
    /// <param name="name">The file name to read</param>
    /// <returns>The file content as byte array, or null if file doesn't exist</returns>
    public async Task<byte[]?> Read(string name)
    {
        var filePath = GetFullFilePath(name);
        
        if (!File.Exists(filePath))
        {
            return null;
        }
        
        try
        {
            return await File.ReadAllBytesAsync(filePath);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Saves data to a file in the file system
    /// </summary>
    /// <param name="name">The file name to save to</param>
    /// <param name="data">The data to save</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task Save(string name, byte[] data)
    {
        var outputDirectory = GetOutputDirectory();
        
        // Ensure output directory exists
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }
        
        var filePath = Path.Combine(outputDirectory, name);
        await File.WriteAllBytesAsync(filePath, data);
    }
    
    /// <summary>
    /// Gets the full file path combining the output directory with the filename
    /// </summary>
    /// <param name="name">The filename (without path)</param>
    /// <returns>The full file path</returns>
    private static string GetFullFilePath(string name)
    {
        return Path.Combine(GetOutputDirectory(), name);
    }
    
    /// <summary>
    /// Gets the output directory path (subfolder 'output' in the executable path)
    /// </summary>
    /// <returns>The full path to the output directory</returns>
    private static string GetOutputDirectory()
    {
        var executablePath = AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(executablePath, "output");
    }
}
