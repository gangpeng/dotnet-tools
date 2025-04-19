using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace DumpAnalyzer;

/// <summary>
/// This class is responsible for processing CLR heap segments.
/// 
/// The class uses a blocking collection to receive CLR segments and 
/// processes them in a separate thread.
/// 
/// To add a new stats process, we need to do three things:
///     (1) Create a new class that implements the IStats interface.
///     (2) Change the options class to add new options for it.
///     (3) Add the processor in the constructor of SegmentWorker.
///     
/// </summary>
internal class SegmentWorker
{
    // Queue to receive CLR segments for processing.
    private BlockingCollection<ClrSegment> _objQueue;

    // Worker ID for logging purposes.
    private int _workerId;

    // List of statistics processors to handle different types of stats.
    private List<IStats> _statsProcessorList = new List<IStats>();

    // Keep track of completed items.
    private volatile int _completedItems = 0;

    public int CompletedItems => _completedItems;

    public SegmentWorker(BlockingCollection<ClrSegment> objQueue, int workerId, Options options)
    {
        _objQueue = objQueue;
        _workerId = workerId;

        if (options.ShowDictStats)
        {
            _statsProcessorList.Add(new DictStats());
        }

        if (options.ShowHolderStats)
        {
            _statsProcessorList.Add(new SpecialObjectHolderStats());
        }

        if (options.ShowHashSetCharStats)
        {
            _statsProcessorList.Add(new HashSetCharCollector(options.MinSize));
        }

        if (options.ShowStringStats)
        {
            _statsProcessorList.Add(new StringCollector(options.MinSize));
        }
    }

    public static Dictionary<string, IStats> AggregateStats(params SegmentWorker[] workers)
    {
        var dictionary = new Dictionary<string, IStats>();
        foreach (var worker in workers)
        {
            foreach (var statsProcessor in worker._statsProcessorList)
            {
                if (dictionary.TryGetValue(statsProcessor.Name, out var otherStats))
                {
                    dictionary[statsProcessor.Name] = statsProcessor.Aggregate(otherStats);
                }
                else
                {
                    dictionary[statsProcessor.Name] = statsProcessor;
                }
            }
        }
        return dictionary;
    }

    internal void ConsumeMessages(CancellationToken token)
    {
        try
        {
            foreach (var segment in _objQueue.GetConsumingEnumerable(token))
            {
                foreach (var obj in segment.EnumerateObjects(false))
                {
                    if (!obj.IsNull)
                    {
                        var type = obj.Type;
                        if (type != null && type.Name != null)
                        {
                            foreach (var statsProcessor in _statsProcessorList)
                            {
                                statsProcessor.ProcessObject(type.Name, obj);
                            }
                        }
                    }
                }
                _completedItems++;
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"[Worker {_workerId}] Cancellation requested. Shutting down.");
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine($"[Worker {_workerId}] Collection disposed or completed unexpectedly.");
        }
    }
}
