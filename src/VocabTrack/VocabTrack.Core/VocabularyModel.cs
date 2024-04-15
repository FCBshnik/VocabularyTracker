using System.Text.Json.Serialization;

namespace VocabTrack.Core
{
    public class VocabularyModel
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
            public bool ContainsWord(string word)
            {
                var symbols = word.ToList().Select(s => s.ToString()).ToList();
                return ContainsWord(symbols, 0);
            }

            public void Add(string word)
            {
                var symbols = word.ToList().Select(s => s.ToString()).ToList();
                Add(symbols, 0);
            }

            private bool ContainsWord(List<string> symbols, int index)
            {
                if (symbols.Count == index)
                    return true;

                var symbol = symbols[index];
                if (!ContainsKey(symbol))
                    return false;

                return this[symbol].ContainsWord(symbols, index + 1);
            }

            private void Add(List<string> symbols, int index)
            {
                if (symbols.Count == index)
                    return;

                var symbol = symbols[index];
                if (!TryGetValue(symbol, out var node))
                {
                    node = new NodeModel();
                    Add(symbol, node);
                }

                node.Add(symbols, index + 1);
            }
        }
    }
}
