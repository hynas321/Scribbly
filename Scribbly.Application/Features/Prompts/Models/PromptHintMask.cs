namespace Scribbly.Application.Features.Prompts;

public static class PromptHintMask
{
    private const char Hidden = '_';

    private static string NormalizeWhitespace(string answer) =>
        string.Join(' ', answer.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    public static string FullMask(string answer) =>
        new(NormalizeWhitespace(answer).Select(c => char.IsLetter(c) ? Hidden : c).ToArray());

    public static List<int> ShuffledRevealOrder(string maskedText, Random random)
    {
        var words = new List<List<int>>();
        int offset = 0;
        foreach (string word in maskedText.Split(' '))
        {
            var positions = new List<int>();
            for (int i = 0; i < word.Length; i++)
            {
                if (word[i] == Hidden)
                {
                    positions.Add(offset + i);
                }
            }
            for (int i = positions.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (positions[i], positions[j]) = (positions[j], positions[i]);
            }
            words.Add(positions.Take(Math.Max(0, positions.Count - 2)).ToList());
            offset += word.Length + 1;
        }
        var queues = words.OrderByDescending(word => word.Count).Select(word => new Queue<int>(word)).ToArray();
        var order = new List<int>();
        while (queues.Any(word => word.Count > 0))
        {
            foreach (var word in queues)
            {
                if (word.TryDequeue(out int position))
                {
                    order.Add(position);
                }
            }
        }
        return order;
    }

    public static string WithRevealedLetters(string answer, IReadOnlyList<int> revealOrder, int revealCount)
    {
        string normalized = NormalizeWhitespace(answer);
        char[] mask = FullMask(normalized).ToCharArray();
        int revealed = 0;
        foreach (int position in revealOrder)
        {
            if (revealed >= revealCount)
            {
                break;
            }
            if (position < 0 || position >= mask.Length || mask[position] != Hidden || !char.IsLetter(normalized[position]))
            {
                continue;
            }
            int start = normalized.LastIndexOf(' ', position) + 1;
            int end = normalized.IndexOf(' ', position);
            end = end < 0 ? mask.Length : end;
            if (mask.Skip(start).Take(end - start).Count(c => c == Hidden) <= 2)
            {
                continue;
            }
            mask[position] = normalized[position];
            revealed++;
        }
        return new string(mask);
    }
}
