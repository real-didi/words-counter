using System.Text;

namespace WordsCounter.Application.Implementations;

public static class ChunksIterator
{
    public static void CountFileWordsInChunksIterator(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(stream);

        const int chunkSize = 1024 * 1;
        var buffer = new char[chunkSize];
        var stringBuilder = new StringBuilder();
        int bytesRead;

        while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
        {
            for (int i = 0; i < bytesRead; i++)
            {
                char currentChar = buffer[i];
                if (Program.Separators.Contains(currentChar))
                {
                    if (stringBuilder.Length > 0 && stringBuilder.Length >= Program.MinWordLength)
                    {
                        string word = stringBuilder.ToString();
                        Program.Stats.AddOrUpdate(word, 1, (key, oldValue) => oldValue + 1);
                        stringBuilder.Clear();
                    }
                }
                else
                {
                    stringBuilder.Append(currentChar);
                }
            }
        }

        if (stringBuilder.Length > 0 && stringBuilder.Length >= Program.MinWordLength)
        {
            string word = stringBuilder.ToString();
            Program.Stats.AddOrUpdate(word, 1, (key, oldValue) => oldValue + 1);
        }
    }
}

