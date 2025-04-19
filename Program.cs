using CommandLine;
using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.IO;

namespace DumpAnalyzer;

internal class Program
{
    private static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(new Action<Options>(RunAnalysis))
            .WithNotParsed(_ => Console.WriteLine("Error parsing command line arguments."));
    }

    private static void RunAnalysis(Options options)
    {
        var output = Console.Out;
        if (!string.IsNullOrEmpty(options.OutputFilePath))
            output = new StreamWriter(options.OutputFilePath);
        try
        {
            Console.WriteLine("Analyzing dump file: " + options.DumpFilePath);
            using (var dataTarget = DataTarget.LoadDump(options.DumpFilePath))
            {
                var clrVersion = dataTarget.ClrVersions[0];
                Console.WriteLine($"CLR Version: {clrVersion.Version}");
                using (var runtime = clrVersion.CreateRuntime())
                {
                    var dumpAnalyzer = new DumpAnalyzer(runtime, output, options);
                    Console.CursorVisible = false;
                    var progressBar = new ConsoleProgressBar();
                    dumpAnalyzer.Run(
                        (completedItems, totalItems) => progressBar.Draw(completedItems, totalItems, "Processing..."));
                    progressBar.Complete("Task finished!");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error analyzing dump: " + ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
        finally
        {
            if (output != Console.Out)
                output.Close();
            Console.CursorVisible = true;
        }
    }
}

