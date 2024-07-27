using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;

namespace WordsCounter.Application;

public class Program
{
    public static string FilesPath = "/Users/user/dev/krtv/tmp/WordsCounter/Files";
    public static int MinWordLength = 5;

    public static readonly char[] Separators = new char[] { ' ', '\r', '\n' };
    public static Regex SeparatorsRegex = new Regex(@"[\s\r\n]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static object Sync = new object();

    public static ConcurrentDictionary<string, int> Stats = new ConcurrentDictionary<string, int>();

    static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Error: parameters required - [1]: files path [2]: Min word length.");
        }

        FilesPath = args[0];

        // new Counter().CountWordsStraightforwardWithChunks();
        new Counter().CountAllFiles();
        //var summary = BenchmarkRunner.Run<Counter>();
    }
}


[MemoryDiagnoser]
[ThreadingDiagnoser]
[IterationCount(3)]
[WarmupCount(3)]
public class Counter
{ 
    static string[] GetFilesByPath(string filePath)
    {
        return Directory.GetFiles(filePath);
    }

    public void CountAllFiles()
    {
        var files = GetFilesByPath(Program.FilesPath);
        Parallel.For(0, files.Length, new ParallelOptions { MaxDegreeOfParallelism = 10 }, i =>
        {
            try
            {
                Implementations.ChunksSplitter.CountFileWordsInChunksSplitter(files[i]);
                Console.WriteLine($"Reading {files[i]} finished.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File '{files[i]}' was skipped due to proccessing error: {ex}.");
            }
        });

        OutputResults();
    }

    static void OutputResults()
    {
        var orderedStats = Program.Stats.OrderByDescending(x => x.Value).Take(10);
        foreach (var pair in orderedStats)
        {
            Console.WriteLine("Total occurrences of {0}: {1}", pair.Key, pair.Value);

        }
        Console.WriteLine("Total word count: {0}", Program.Stats.Count);
    }

    [GlobalSetup]
    [IterationCleanup]
    public void Cleanup()
    {
        lock (Program.Sync)
            Program.Stats.Clear();
    }

    [Benchmark]
    public void CountFileWordsUsingSpans()
    {
        var files = GetFilesByPath(Program.FilesPath);
        //var stats = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        Implementations.Spans.CountFileWordsUsingSpans(files[1]);
        OutputResults();
    }

    [Benchmark]
    public void CountFileWordsInChunksUsingSpans()
    {
        var files = GetFilesByPath(Program.FilesPath);
        //var stats = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        Implementations.SpansWithChunks.CountFileWordsInChunksUsingSpans(files[1]);
        OutputResults();
    }

    [Benchmark]
    public void CountFileWordsInChunksIterator()
    {
        var files = GetFilesByPath(Program.FilesPath);
        //var stats = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        Implementations.ChunksIterator.CountFileWordsInChunksIterator(files[1]);
        OutputResults();
    }

    [Benchmark]
    public void CountFileWordsInChunksSplitter()
    {
        var files = GetFilesByPath(Program.FilesPath);
        //var stats = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        Implementations.ChunksSplitter.CountFileWordsInChunksSplitter(files[1]);
        OutputResults();
    }

    [Benchmark]
    public void CountFileWordsInWholeFile()
    {
        var files = GetFilesByPath(Program.FilesPath);
        //var stats = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        Implementations.WholeFile.CountFileWordsInWholeFile(files[1]);
        OutputResults();
    }


}


//https://stackoverflow.com/questions/65202898/count-how-many-times-certain-words-appear-in-a-text-with-c-sharp
//var file = @"loremIpsum.txt";
//var obj = new object();
//var wordsToMatch = new ConcurrentDictionary<string, int>();

//wordsToMatch.TryAdd("Lorem", 0);
//    wordsToMatch.TryAdd("ipsum", 0);
//    wordsToMatch.TryAdd("amet", 0);

//    Console.WriteLine("Press a key to continue...");
//    Console.ReadKey();

//    Parallel.ForEach(File.ReadLines(file),
//        (line) =>
//        {
//            foreach (var word in wordsToMatch.Keys)
//                lock (obj)
//                    wordsToMatch[word] += Regex.Matches(line, word,
//                        RegexOptions.IgnoreCase).Count;
//        });

//foreach (var kv in wordsToMatch.OrderByDescending(x => x.Value))
//    Console.WriteLine($"Total occurrences of {kv.Key}: {kv.Value}");

//Console.WriteLine($"Total word count: {wordsToMatch.Values.Sum()}");
//Console.ReadKey();