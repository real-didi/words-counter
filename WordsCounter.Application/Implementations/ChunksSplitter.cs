using System.Text;

namespace WordsCounter.Application.Implementations;

public static class ChunksSplitter
{
    public static void CountFileWordsInChunksSplitter(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(stream);

        const int chunkSize = 1024 * 40;
        var buffer = new char[chunkSize];
        var stringBuilder = new StringBuilder(chunkSize);
        int bytesRead;

        while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
        {
            string chunk = new string(buffer, 0, bytesRead);

            // Using regex code below increases memory size by 40% and run time by 50%, so string.Split is used instead
            //var words = Program.SeparatorsRegex.Split(chunk);

            var words = chunk.Split(Program.Separators);

            // No delimeters found, so appending chars for the next chunk
            if (words.Length == 1)
            {
                stringBuilder.Append(words[0]);
            }
            else
            {
                // Concat first found word with previous chunks leftover
                stringBuilder.Append(words[0]);
                words[0] = stringBuilder.ToString();
                stringBuilder.Clear();

                // Skipping last as we're not sure if it is delimited 
                for (var i = 0; i < words.Length - 1; i++)
                {
                    var word = words[i];

                    if (!string.IsNullOrWhiteSpace(word) && word.Length >= Program.MinWordLength)
                    {
                        
                        Program.Stats.AddOrUpdate(word, 1, (key, oldValue) => oldValue + 1);
                    }
                }

                // Append last word to StringBuilder for the next chunk
                stringBuilder.Append(words[words.Length - 1]);
            }
        }

        if (stringBuilder.Length > 0)
        {
            string word = stringBuilder.ToString();
            if (!string.IsNullOrWhiteSpace(word) && word.Length >= Program.MinWordLength)
            {
                Program.Stats.AddOrUpdate(word, 1, (key, oldValue) => oldValue + 1);
            }
        }
    }
}

