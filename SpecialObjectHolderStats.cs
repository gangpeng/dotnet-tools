using Microsoft.Diagnostics.Runtime;
using System.IO;
using System.Linq;
using System;

namespace DumpAnalyzer;

/// <summary>
/// This class tries to find which types are top users a some other types (such as Di
/// </summary>
internal class SpecialObjectHolderStats : IStats
{
    public Histogram<string> _emptyDictHolderHistogram = new Histogram<string>();
    public Histogram<string> _byteArrayHolderHistogram = new Histogram<string>();

    public string Name => "Special Object Holder Stats";

    public void ProcessObject(string typeName, ClrObject obj)
    {
        foreach (ClrInstanceField field in obj.Type!.Fields)
        {
            if (field.IsObjectReference)
            {
                ClrObject clrObject = field.ReadObject(obj.Address, false);
                if (!clrObject.IsValid)
                {
                    continue;
                }

                // valid object will have non-null type
                var type = clrObject.Type;
                var fieldTypeName = type!.Name;
                if (String.IsNullOrEmpty(fieldTypeName))
                {
                    continue;
                }

                if (fieldTypeName.Equals("System.Byte[]"))
                    _byteArrayHolderHistogram.Add(typeName);
                else if (DictStats.IsDictionaryType(fieldTypeName) 
                    && DictStats.GetDictionarySize(clrObject) == 0)
                    _emptyDictHolderHistogram.Add(typeName);
            }
        }
    }

    public IStats Aggregate(IStats otherStats)
    {
        if (!(otherStats is SpecialObjectHolderStats objectHolderStats))
            return this;
        
        foreach (var bucket in objectHolderStats._emptyDictHolderHistogram.Buckets)
            _emptyDictHolderHistogram.Add(bucket.Key, bucket.Value);

        foreach (var bucket in objectHolderStats._byteArrayHolderHistogram.Buckets)
            _byteArrayHolderHistogram.Add(bucket.Key, bucket.Value);
        
        return this;
    }

    public void Print(TextWriter output)
    {
        output.WriteLine("Special Object Holder Statistics:");
        
        output.WriteLine($"Total unique types holding empty dictionaries: {_emptyDictHolderHistogram.Buckets.Count()}");
        output.WriteLine("Histogram of types holding empty dictionaries:");
        _emptyDictHolderHistogram.Print(output);

        output.WriteLine();
        output.WriteLine($"Total unique types holding byte arrays: {_byteArrayHolderHistogram.Buckets.Count()}");
        output.WriteLine("Histogram of types holding byte arrays:");
        _byteArrayHolderHistogram.Print(output);
    }
}
