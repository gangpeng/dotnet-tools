using CommandLine;

namespace DumpAnalyzer;

/// <summary>
/// Command line options for the dump analyzer.
/// </summary>
public class Options
{
    [Option('f', "file", Required = true, HelpText = "Path to the dump file to analyze.")]
    public string DumpFilePath { get; set; } = string.Empty;

    [Option('o', "output", Required = false, HelpText = "Output file path for the analysis results.")]
    public string? OutputFilePath { get; set; }

    [Option('d', "dictstats", Required = false, HelpText = "Show dictionary size statistics.")]
    public bool ShowDictStats { get; set; } = false;

    [Option('h', "hashsetstats", Required = false, HelpText = "Show hash set char statistics.")]
    public bool ShowHashSetCharStats { get; set; } = false;

    [Option('s', "stringstats", Required = false, HelpText = "Show string value statistics.")]
    public bool ShowStringStats { get; set; } = false;

    [Option('c', "holderstats", Required = false, HelpText = "Show type that contain special objects statistics.")]
    public bool ShowHolderStats { get; set; } = false;

    [Option('l', "segmentlimits", Required = false, HelpText = "Number of segments to process before we stop.")]
    public int SegmentsLimits { get; set; } = int.MaxValue;

    [Option('m', "minsize", Required = false, HelpText = "min size of objects to consider.")]
    public int MinSize { get; set; } = 0;
}
