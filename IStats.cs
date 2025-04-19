
using Microsoft.Diagnostics.Runtime;
using System.IO;

namespace DumpAnalyzer;

/// <summary>
/// The interface to implement for any stats collector.
/// </summary>
public interface IStats
{
    /// <summary>
    /// This method will be called for each object in the heap. 
    /// 
    /// We should expect the typeName and obj to be valid.
    /// </summary>
    /// <param name="typeName"></param>
    /// <param name="obj"></param>
    void ProcessObject(string typeName, ClrObject obj);

    /// <summary>
    /// Print the stats to the output stream.
    /// </summary>
    /// <param name="output"></param>
    void Print(TextWriter output);

    /// <summary>
    /// The name of the stats collector. This is used to aggregate the stats from multiple workers.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Method to aggregate the stats with the other. The current instance will be updated in place.
    /// </summary>
    /// <param name="otherStats"></param>
    /// <returns></returns>
    IStats Aggregate(IStats otherStats);
}
