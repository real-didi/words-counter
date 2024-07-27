namespace WordsCounter.Application.Implementations;

public static class WholeFile
{
    public static void CountFileWordsInWholeFile(string filePath)
    {
        Console.WriteLine($"Processing file: {filePath}.");

        using TextReader reader = File.OpenText(filePath);

        string line = reader.ReadToEnd();
        var words = line.Split(Program.Separators);

        foreach (var word in words)
        {
            if (word.Length < Program.MinWordLength)
            {
                continue;
            }

            if (!Program.Stats.TryAdd(word, 1))
            {
                lock (Program.Sync)
                    Program.Stats[word] += 1;
            }
        }
    }
}

