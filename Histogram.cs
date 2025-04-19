using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DumpAnalyzer;

/// <summary>
/// A simple histogram class to track the frequency of items.
/// </summary>
/// <typeparam name="T"></typeparam>
public class Histogram<T> where T : notnull
{
    /// <summary>
    /// Number of top items to print.
    /// </summary>
    private const int TopCount = 20;

    /// <summary>
    /// When we print strings in console, we need to limit the length of the string.
    /// </summary>
    private const int StringLength = 80;

    const int PercentageWidth = 10;

    const int ValueWidth = 16;

    private readonly Dictionary<T, int> _counts = new Dictionary<T, int>();

    public void Add(T item)
    {
        if (_counts.ContainsKey(item))
            _counts[item]++;
        else
            _counts[item] = 1;
    }

    public void Add(T item, int value)
    {
        _counts[item] = _counts.GetValueOrDefault(item) + value;
    }

    public int this[T item]
    {
        get
        {
            return !_counts.TryGetValue(item, out var num) ? 0 : num;
        }
    }

    public IEnumerable<KeyValuePair<T, int>> Buckets
    {
        get => _counts;
    }

    public void Print(TextWriter output, int topCount = TopCount, int stringLength = StringLength)
    {
        var totalValue = _counts.Values.Sum();
        var keyWidth = stringLength;
        output.WriteLine($"{"Key".PadRight(keyWidth)} {"Value".PadRight(ValueWidth)} {"Percentage"}");
        output.WriteLine(new string('-', keyWidth + ValueWidth + PercentageWidth));
        foreach (var keyValuePair in _counts.OrderByDescending(kv => kv.Value).Take(topCount))
        {
            var key = TruncateString(keyValuePair.Key.ToString(), stringLength);
            var value = keyValuePair.Value.ToString().PadRight(ValueWidth);
            var percentage = totalValue <= 0 ? "0.000%" : $"{(double)keyValuePair.Value * 100.0 / totalValue:F2}%";
            output.WriteLine($"{key.PadRight(stringLength)} {value} {percentage.PadRight(PercentageWidth)}");
        }
    }

    private string TruncateString(string? input, int maxLength)
    {
        return string.IsNullOrEmpty(input) ? string.Empty : 
            (input.Length <= maxLength ? input : input.Substring(0, maxLength - 3) + "...");
    }
}
