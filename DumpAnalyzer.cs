using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DumpAnalyzer;

/// <summary>
/// Interface for reporting progress.
/// </summary>
/// <param name="completedItems"></param>
/// <param name="totalItems"></param>
public delegate void ProgressReporter(int completedItems, int totalItems);

/// <summary>
/// Class for analyzing a memory dump and reporting progress in console.
/// </summary>
public class DumpAnalyzer
{
    /// <summary>
    /// Walking the entire heap is slow, we need multiple threads to fully utilize the CPU.
    /// The value is picked based on simple test.
    /// </summary>
    private static int WorkerCount = Environment.ProcessorCount * 3;

    const int ProgressReportingMillseconds = 5000;

    private readonly ClrRuntime _runtime;
    private readonly TextWriter _output;
    private readonly Options _options;

    public DumpAnalyzer(ClrRuntime runtime, TextWriter output, Options options)
    {
        _runtime = runtime;
        _output = output;
        _options = options;
    }

    public void Run(ProgressReporter reporter)
    {
        var blockingCollection = new BlockingCollection<ClrSegment>();
        var tokenSource = new CancellationTokenSource();
        var workers = new SegmentWorker[WorkerCount];
        var source = new Task[workers.Length];

        for (var workerId = 0; workerId < workers.Length; ++workerId)
        {
            var worker = new SegmentWorker(blockingCollection, workerId, _options);
            workers[workerId] = worker;
            source[workerId] = Task.Run((Action)(() => worker.ConsumeMessages(tokenSource.Token)));
        }

        var queue = addSegmentsToQueue(blockingCollection);
        while (!source.All(t => t.IsCompleted))
        {
            var completedItems = 0;
            for (var index = 0; index < workers.Length; ++index)
                completedItems += workers[index].CompletedItems;

            reporter(completedItems, queue);

            if (completedItems < queue)
                Thread.Sleep(ProgressReportingMillseconds);
            else
                break;
        }

        foreach (var aggregateStat in SegmentWorker.AggregateStats(workers))
        {
            _output.WriteLine($"Stats: {aggregateStat.Key}:");
            _output.WriteLine("====================");
            aggregateStat.Value.Print(_output);
        }
    }

    private int addSegmentsToQueue(BlockingCollection<ClrSegment> segmentQueue)
    {
        var queue = 0;
        foreach (var segment in _runtime.Heap.Segments)
        {
            if (queue < _options.SegmentsLimits)
            {
                segmentQueue.Add(segment);
                ++queue;
            }
            else
                break;
        }
        segmentQueue.CompleteAdding();
        return queue;
    }
}
