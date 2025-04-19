using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DumpAnalyzer;

/// <summary>
/// This class checks if we are duplicate strings.
/// </summary>
internal class StringCollector : IStats
{
    private Histogram<string> _stringHistogram = new Histogram<string>();
    private int _minSize;

    public StringCollector(int minSize) => _minSize = minSize;

    public string Name => "String Object Stats";

    public void ProcessObject(string typeName, ClrObject obj)
    {
        if (!typeName.Equals("System.String"))
            return;

        var str = obj.AsString(Int32.MaxValue);
        if (str == null || str.Length < _minSize)
            return;

        _stringHistogram.Add(str);
    }

    public IStats Aggregate(IStats otherCollector)
    {
        if (otherCollector is StringCollector stringCollector)
        {
            foreach (var bucket in stringCollector._stringHistogram.Buckets)
                _stringHistogram.Add(bucket.Key, bucket.Value);
        }
        return this;
    }

    public void Print(TextWriter output)
    {
        output.WriteLine("String Collector Statistics:");
        output.WriteLine($"Total unique strings: {_stringHistogram.Buckets.Count()}");
        output.WriteLine("Histogram of strings:");
        _stringHistogram.Print(output);
    }
}
