using Microsoft.Diagnostics.Runtime;
using System.Collections.Generic;
using System.IO;

namespace DumpAnalyzer;

/// <summary>
/// This class collects statistics about the size of dictionaries in the heap.
/// </summary>
internal class DictStats : IStats
{
    private long _numberOfDicts = 0;
    private long _totalEntries = 0;
    private Histogram<int> _histogram = new Histogram<int>();

    public string Name => "Dictonary Size Stats";

    public static bool IsDictionaryType(string typeName)
    {
        return typeName.StartsWith("System.Collections.Generic.Dictionary<") && typeName.EndsWith(">");
    }

    public static int GetDictionarySize(ClrObject obj)
    {
        // this is based on the implementation details of .Net Dictionary. 
        return obj.ReadField<int>("_count") - obj.ReadField<int>("_freeCount");
    }

    public IStats Aggregate(IStats otherStats)
    {
        if (!(otherStats is DictStats dictStats))
        {
            return this;
        }
        _numberOfDicts += dictStats._numberOfDicts;
        _totalEntries += dictStats._totalEntries;
        foreach (KeyValuePair<int, int> bucket in dictStats._histogram.Buckets)
        {
            _histogram.Add(bucket.Key, bucket.Value);
        }
        return this;
    }

    public void ProcessObject(string typeName, ClrObject obj)
    {
        if (!DictStats.IsDictionaryType(typeName))
        {
            return;
        }

        ++_numberOfDicts;
        int size = DictStats.GetDictionarySize(obj);
        ClrObject clrObject = obj.ReadObjectField("_entries");
        int entries = 0;
        if (!clrObject.IsNull)
        {
            entries = clrObject.AsArray().Length;
        }
        _histogram.Add(size);
        _totalEntries += (long)entries;
    }

    public void Print(TextWriter output)
    {
        output.WriteLine("Print Dictionary Size Stats");
        output.WriteLine($"Number of dictionaries: {_numberOfDicts}");
        output.WriteLine($"Total entries: {_totalEntries}");
        var averageEntries = _numberOfDicts == 0L ? 0.0 : (double)_totalEntries / _numberOfDicts;
        output.WriteLine($"Average entries per dictionary: {averageEntries:F2}");

        long totalSize = 0;
        long numberOfNonEmptyDicts = this._numberOfDicts;
        foreach (KeyValuePair<int, int> bucket in this._histogram.Buckets)
        {
            totalSize += (long)bucket.Key * (long)bucket.Value;
            if (bucket.Key == 0)
            {
                numberOfNonEmptyDicts -= (long)bucket.Value;
            }
        }
        double loadFactor = numberOfNonEmptyDicts == 0L ? 0.0 : (double)totalSize / numberOfNonEmptyDicts;
        output.WriteLine($"Average load factor: {loadFactor:F2}");
        output.WriteLine("Histogram of dictionary sizes:");
        _histogram.Print(output);
    }
}
