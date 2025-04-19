using Microsoft.Diagnostics.Runtime;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DumpAnalyzer;

/// <summary>
/// Collect the stats about HashSet<char> objects.
/// 
/// We will convert HashSet<char> to a string and see if there are duplicates.
/// </summary>
internal class HashSetCharCollector : IStats
{
    private Histogram<string> _setHistogram = new Histogram<string>();
    private Histogram<int> _sizeHistogram = new Histogram<int>();

    /// <summary>
    /// The minimum size of the HashSet<char> to be considered for collection.
    /// </summary>
    private int _minSize;

    public HashSetCharCollector(int minSize) => _minSize = minSize;

    public string Name => "HashSet<Char> Stats";

    public static string ConvertHashSetToSortedString(HashSet<char> charSet)
    {
        if (charSet == null || charSet.Count == 0)
            return string.Empty;
        var list = charSet.ToList();
        list.Sort();
        return new string(list.ToArray());
    }

    public void ProcessObject(string typeName, ClrObject obj)
    {
        if (!typeName.Equals("System.Collections.Generic.HashSet<System.Char>"))
            return;

        var count = obj.ReadField<int>("_count");
        if (count == 0)
            return;

        var freeCount = obj.ReadField<int>("_freeCount");
        if (count - freeCount < _minSize)
            return;

        var clrObject = obj.ReadObjectField("_entries");
        if (!clrObject.IsValid)
            return;

        var clrArray = clrObject.AsArray();
        var charSet = new HashSet<char>(count - freeCount);
        var index = 0;
        while ((uint)index < (uint)count)
        {
            var structValue = clrArray.GetStructValue(index++);
            if (structValue.ReadField<int>("Next") >= -1)
            {
                var ch = structValue.ReadField<char>("Value");
                charSet.Add(ch);
            }
        }
        _setHistogram.Add(ConvertHashSetToSortedString(charSet));
        _sizeHistogram.Add(charSet.Count);
    }

    public IStats Aggregate(IStats otherCollector)
    {
        if (!(otherCollector is HashSetCharCollector setCharCollector))
            return this;

        foreach (var bucket in setCharCollector._setHistogram.Buckets)
            _setHistogram.Add(bucket.Key, bucket.Value);

        foreach (var bucket in setCharCollector._sizeHistogram.Buckets)
            _sizeHistogram.Add(bucket.Key, bucket.Value);

        return this;
    }

    public void Print(TextWriter output)
    {
        output.WriteLine("HashSet<char> Collector Statistics:");
        output.WriteLine($"Total unique HashSet<char>: {_setHistogram.Buckets.Count()}");
        output.WriteLine("Histogram of string values:");
        _setHistogram.Print(output);

        output.WriteLine("Histogram of set size:");
        _sizeHistogram.Print(output);
    }
}

