using System.Text;
using System.Text.Json.Serialization;

namespace VocabTrack.Core
{
    public class Vocabulary
    {
        [JsonPropertyName("subs")]
        public Dictionary<string, string> Subs { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        [JsonPropertyName("words")]
        public NodeModel Words { get; set; } = new NodeModel();

        public class WordInfo
        {
            public string? Note { get; set; }

            public long Occurrences { get; set; }
        }

        public class NodeModel : Dictionary<string, NodeModel?>
        {
            public Dictionary<string, WordInfo> ListWords()
            {
                var words = new Dictionary<string, WordInfo>();
                ListWords(words, new StringBuilder());
                return words;
            }

            public void IncrementWordOccurrences(string word, int occurrences = 1)
            {
                var symbols = word.ToLowerInvariant().ToList().Select(s => s.ToString()).ToList();
                AddWord(symbols, 0, occurrences);
            }

            public void SetWordNote(string word, string note)
            {
                var symbols = word.ToLowerInvariant().ToList().Select(s => s.ToString()).ToList();
                SetWordNote(symbols, 0, note);
            }

            private void SetWordNote(List<string> symbols, int index, string note)
            {
                var isLast = index == symbols.Count - 1;
                var symbol = symbols[index];

                if (!TryGetValue(symbol, out var child))
                    throw new InvalidOperationException($"word {string.Join(string.Empty, symbols)} not found in vocabulary");

                if (isLast)
                {
                    var key = child!.Keys.FirstOrDefault(k => k.StartsWith("@"));
                    if (key != null)
                        child!.Remove(key);
                    child!.Add($"@{note}", null);
                    return;
                }

                child!.SetWordNote(symbols, index + 1, note);
            }

            private void ListWords(Dictionary<string, WordInfo> words, StringBuilder builder)
            {
                foreach (var letter in Keys)
                {
                    if (letter.Length > 1)
                        continue;

                    builder.Append(letter);

                    var occursKey = this[letter]!.Keys.FirstOrDefault(k => k.StartsWith("#"));
                    if (occursKey != null)
                    {
                        words.Add(builder.ToString().ToLowerInvariant(), new WordInfo
                        {
                            Note = this[letter]!.Keys.FirstOrDefault(k => k.StartsWith("@"))?.Replace("@", string.Empty),
                            Occurrences = long.Parse(occursKey.Substring(1))
                        });
                    }

                    this[letter]!.ListWords(words, builder);

                    builder.Remove(builder.Length - 1, 1);
                }
            }

            private void AddWord(List<string> symbols, int index, int occurrences)
            {
                if (symbols.Count == index)
                    return;

                var isLast = index == symbols.Count - 1;
                var symbol = symbols[index];

                if (!TryGetValue(symbol, out var child))
                {
                    child = new NodeModel();
                    Add(symbol, child);
                    // add initial occurrences
                    if (isLast)
                        child.Add($"#{occurrences}", null);
                }
                else if (isLast)
                {
                    var key = child!.Keys.FirstOrDefault(k => k.StartsWith("#"));
                    // add initial occurrences
                    if (key == null)
                    {
                        child.Add($"#{occurrences}", null);
                    }
                    // or increment occurrences
                    else
                    {
                        child!.Remove(key);
                        child!.Add($"#{long.Parse(key.Substring(1)) + occurrences}", null);
                    }
                }

                child!.AddWord(symbols, index + 1, occurrences);
            }
        }
    }
}
