namespace AgentExtraction.Storage;

/// <summary>
/// Interface for agnostic storage operations
/// </summary>
public interface IStorage
{
    /// <summary>
    /// Reads data from storage by name
    /// </summary>
    /// <param name="name">The name of the data to read</param>
    /// <returns>Byte array containing the data, or null if not found</returns>
    Task<byte[]?> Read(string name);

    /// <summary>
    /// Saves data to storage with the specified name
    /// </summary>
    /// <param name="name">The name to save the data as</param>
    /// <param name="data">The data to save</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task Save(string name, byte[] data);
}
