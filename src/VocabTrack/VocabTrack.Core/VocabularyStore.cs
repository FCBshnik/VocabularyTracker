using System.Text.Json;

namespace VocabTrack.Core
{
    public static class VocabularyStore
    {
        private static readonly string fileName = "vocab.json";

        public static VocabularyModel Load()
        {
            var file = new FileInfo(fileName);
            if (!file.Exists)
            {
                Console.WriteLine($"new vocabulary created");
                return new VocabularyModel();
            }

            var json = File.ReadAllText(file.FullName);
            return JsonSerializer.Deserialize<VocabularyModel>(json)!;
        }

        public static void Save(VocabularyModel vocabulary)
        {
            var json = JsonSerializer.Serialize(vocabulary, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine($"{json}");
            File.WriteAllText(fileName, json);
        }
    }
}
