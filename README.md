# DumpAnalyzer

`DumpAnalyzer` is a .NET tool for analyzing memory dump files. It provides insights into the contents of a memory dump, such as dictionary statistics, hash set character statistics, string value statistics, and more. The tool is designed to process large heaps efficiently using multiple threads and provides progress updates during the analysis.

## Features

- **Dictionary Statistics**: Analyze the size and count of dictionaries in the heap.
- **HashSet<char> Statistics**: Collect and analyze statistics about `HashSet<char>` objects.
- **String Value Statistics**: Gather statistics about string values in the heap.
- **Special Object Holder Statistics**: Identify types that hold special objects, such as empty dictionaries or large byte arrays.
- **Segment Limits**: Limit the number of heap segments to process.
- **Progress Reporting**: Displays progress updates in the console during analysis.

## Command-Line Options

The following command-line options are available:

| Option                  | Description                                                                                     | Required | Default Value       |
|-------------------------|-------------------------------------------------------------------------------------------------|----------|---------------------|
| `-f, --file`            | Path to the dump file to analyze.                                                              | Yes      | N/A                 |
| `-o, --output`          | Output file path for the analysis results.                                                     | No       | Console output      |
| `-d, --dictstats`       | Show dictionary size statistics.                                                               | No       | `false`             |
| `-h, --hashsetstats`    | Show hash set character statistics.                                                            | No       | `false`             |
| `-s, --stringstats`     | Show string value statistics.                                                                  | No       | `false`             |
| `-c, --holderstats`     | Show types that contain special objects (e.g., empty dictionaries, large byte arrays).         | No       | `false`             |
| `-l, --segmentlimits`   | Number of heap segments to process before stopping.                                            | No       | `int.MaxValue`      |
| `-m, --minsize`         | Minimum size of objects to consider during analysis.                                           | No       | `0`                 |

## Usage

### Basic Usage
To analyze a memory dump file and show string value stats:

`dotnet DumpAnalyzer.dll -s -f path/to/dumpfile.dmp`

### Example with Options
To analyze a dump file and output dictionary statistics to a file:

`dotnet DumpAnalyzer.dll -f path/to/dumpfile.dmp -o results.txt -d`

To analyze a dump file and show hash set character statistics with a minimum size of 10:

`dotnet DumpAnalyzer.dll -f path/to/dumpfile.dmp -h -m 10`


### Progress Reporting
The tool provides progress updates in the console during analysis, showing the number of completed items and the total items to process.

## How It Works

1. **Heap Segments**: The tool processes heap segments from the memory dump file.
2. **Multi-Threading**: Multiple worker threads are used to analyze the heap in parallel, improving performance.
3. **Statistics Collection**: Each worker collects statistics based on the enabled options (e.g., dictionary stats, string stats).
4. **Aggregation**: The results from all workers are aggregated and displayed or written to the output file.

## Requirements

- .NET 6 Runtime
- A valid memory dump file to analyze.

## License

This project is licensed under the terms of the [LICENSE](LICENSE) file.


