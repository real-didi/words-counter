using System.Text;

namespace WordsCounter.Application.Implementations;

public static class Spans
{
    public static void CountFileWordsUsingSpans(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(stream);

        const int chunkSize = 1;// 8192 / 2;

        var buffer = new char[chunkSize].AsSpan();

        var separators = Program.Separators.AsSpan();

        var stringBuilder = new StringBuilder(chunkSize);
        int bytesRead;
        int separatorIndex = -1;

        while ((bytesRead = reader.ReadBlock(buffer)) > 0)
        {
            separatorIndex = buffer.IndexOfAny(separators);

            if (separatorIndex == -1)
            {
                stringBuilder.Append(buffer);
                continue;
            }

            if (stringBuilder.Length >= Program.MinWordLength)
            {
                Program.Stats.AddOrUpdate(stringBuilder.ToString(), 1, (key, oldValue) => oldValue + 1);
            }
            stringBuilder.Clear();
        }

        if (stringBuilder.Length > 0 && stringBuilder.Length >= Program.MinWordLength)
        {
            Program.Stats.AddOrUpdate(stringBuilder.ToString(), 1, (key, oldValue) => oldValue + 1);
        }
    }
}

