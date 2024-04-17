using System.Text;
using System.Text.Json.Serialization;

namespace VocabTrack.Core
{
    public class Vocabulary
    {
        [JsonPropertyName("learned")]
        public NodeModel Learned { get; set; } = new NodeModel();

        [JsonPropertyName("names")]
        public NodeModel Names { get; set; } = new NodeModel();

        [JsonPropertyName("todo")]
        public NodeModel Todo { get; set; } = new NodeModel();

        public bool ContainsWord(string word)
        {
            return Learned.ContainsWord(word) || Names.ContainsWord(word) || Todo.ContainsWord(word);
        }

        public class NodeModel : Dictionary<string, NodeModel>
        {
            public List<string> ListWords()
            {
                var words = new List<string>();
                ListWords(words, new StringBuilder());
                return words;
            }

            public bool ContainsWord(string word)
            {
                var symbols = word.ToLowerInvariant().ToList().Select(s => s.ToString()).ToList();
                return ContainsWord(symbols, 0);
            }

            public void AddWord(string word)
            {
                var symbols = word.ToLowerInvariant().ToList().Select(s => s.ToString()).ToList();
                AddWord(symbols, 0);
            }

            public void RemoveWord(string word)
            {
                var symbols = word.ToLowerInvariant().ToList().Select(s => s.ToString()).ToList();
                RemoveWord(symbols, 0);
            }

            private void ListWords(List<string> words, StringBuilder builder)
            {
                foreach (var letter in Keys)
                {
                    builder.Append(letter);

                    if (char.IsUpper(letter[0]))
                        words.Add(builder.ToString().ToLowerInvariant());

                    this[letter].ListWords(words, builder);

                    builder.Remove(builder.Length - 1, 1);
                }
            }

            private bool ContainsWord(List<string> symbols, int index)
            {
                if (symbols.Count == index)
                    return true;

                var isLast = index == symbols.Count - 1;
                var lower = symbols[index];
                var upper = lower.ToUpperInvariant();

                if (!TryGetValue(upper, out var child))
                {
                    if (!TryGetValue(lower, out child))
                    {
                        return false;
                    }
                    else
                    {
                        if (isLast)
                            return false;
                    }
                }

                return child.ContainsWord(symbols, index + 1);
            }

            private bool RemoveWord(List<string> symbols, int index)
            {
                var isLast = index == symbols.Count - 1;
                var lower = symbols[index];
                var upper = lower.ToUpperInvariant();

                if (isLast)
                    return Remove(upper);

                if (!TryGetValue(lower, out var child) && !TryGetValue(upper, out child))
                    return false;

                if (!child.RemoveWord(symbols, index + 1))
                    return false;

                return Remove(lower);
            }

            private void AddWord(List<string> symbols, int index)
            {
                if (symbols.Count == index)
                    return;

                var isLast = index == symbols.Count - 1;
                var lower = symbols[index];
                var upper = lower.ToUpperInvariant();

                if (!TryGetValue(upper, out var child))
                {
                    if (!TryGetValue(lower, out child))
                    {
                        child = new NodeModel();
                        Add(isLast ? upper : lower, child);
                    }
                    else
                    {
                        if (isLast)
                        {
                            Remove(lower);
                            Add(upper, child);
                        }
                    }
                }

                child.AddWord(symbols, index + 1);
            }
        }
    }
}
