namespace TransactionService.Application.Utilities;

public class TextChunker
{
    public static List<string> SplitText(string text, int chunkSize = 500, int overlap = 50)
    {
        var chunks = new List<string>();

        if (string.IsNullOrWhiteSpace(text))
        {
            return chunks;
        }

        var words = text.Split(new[] {' ', '\r', '\t', '\n'}, StringSplitOptions.RemoveEmptyEntries);

        if(words.Length <= chunkSize)
        {
            chunks.Add(string.Join(" ", words));
            return chunks;
        }

        int step = chunkSize - overlap;

        for(int i = 0; i < words.Length; i+= step)
        {
            var chunkWords = words.Skip(i).Take(chunkSize);

            chunks.Add(string.Join(" ", chunkWords));

            if(i + chunkSize >= words.Length)
            {
                break;
            }
        }

        return chunks;
    }
}