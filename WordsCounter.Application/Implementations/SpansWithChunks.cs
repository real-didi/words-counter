using System.Text;

namespace WordsCounter.Application.Implementations;

public static class SpansWithChunks
{
    public static void CountFileWordsInChunksUsingSpans(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(stream);

        const int chunkSize = 1024 * 16;// 8192 / 2;

        var buffer = new char[chunkSize].AsSpan();
        var tempBuffer = new char[chunkSize].AsSpan();

        var mergeBuffer = new Span<char>();
        var emptySpan = new Span<char>();

        var separators = Program.Separators.AsSpan();

        var stringBuilder = new StringBuilder(chunkSize);
        int bytesRead;
        int chunkCount = 0;
        int separatorIndex = -1;

        while ((bytesRead = reader.ReadBlock(buffer)) > 0)
        {
            ++chunkCount;
            if (chunkCount == 6168379)
            {

            }
            separatorIndex = buffer.IndexOfAny(separators);

            // No words found, merging this piece to mergeBuffer for later processing
            if (separatorIndex == -1)
            {
                mergeBuffer = ConcatSpans(mergeBuffer, buffer);
                continue;
            }

            tempBuffer = buffer[..];

            while (separatorIndex > 0)
            {
                mergeBuffer = ConcatSpans(mergeBuffer, tempBuffer.Slice(0, separatorIndex));
                if (mergeBuffer.Length > 0 && mergeBuffer.Length >= Program.MinWordLength)
                {
                    Program.Stats.AddOrUpdate(mergeBuffer.ToString(), 1, (key, oldValue) => oldValue + 1);
                }
                mergeBuffer = emptySpan;

                tempBuffer = tempBuffer.Length <= separatorIndex + 1 ? emptySpan : tempBuffer.Slice(separatorIndex + 1, tempBuffer.Length - separatorIndex - 1);
                separatorIndex = tempBuffer.IndexOfAny(separators);
            }

            if (tempBuffer.Length > 0)
            {
                mergeBuffer = CopySpan(tempBuffer);
            }
        }

        // Merged piece left, adding it as a word
        if (mergeBuffer.Length > 0 && mergeBuffer.Length >= Program.MinWordLength)
        {
            Program.Stats.AddOrUpdate(mergeBuffer.ToString(), 1, (key, oldValue) => oldValue + 1);
        }
    }

    static Span<char> CopySpan(Span<char> span)
    {
        Memory<char> combinedMemory = new char[span.Length];
        Span<char> combinedSpan = combinedMemory.Span;
        span.CopyTo(combinedSpan);

        return combinedSpan;
    }

    static Span<char> ConcatSpans(Span<char> span1, Span<char> span2)
    {
        Memory<char> combinedMemory = new char[span1.Length + span2.Length];
        Span<char> combinedSpan = combinedMemory.Span;
        span1.CopyTo(combinedSpan);
        span2.CopyTo(combinedSpan.Slice(span1.Length));

        return combinedSpan;
    }
}

