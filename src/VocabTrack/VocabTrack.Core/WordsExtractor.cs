namespace VocabTrack.Core
{
    public class WordsExtractor
    {
        private static readonly char[] wordDelimiters = { ' ', '.', ',', '!', '?' };

        public Dictionary<string, int> ExtractWords(List<string> lines)
        {
            var words = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var line in lines)
            {
                var tokens = line
                    .Split(wordDelimiters)
                    .Select(t => t.Trim().ToLowerInvariant())
                    .Where(t => t.Length > 1 && t.Any(c => char.IsLetter(c)));

                foreach (var token in tokens)
                {
                    if (!words.TryAdd(token, 1))
                        words[token]++;
                }
            }

            return words;
        }
    }
}
