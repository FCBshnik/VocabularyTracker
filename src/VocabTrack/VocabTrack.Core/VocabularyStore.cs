using System.Text.Json;

namespace VocabTrack.Core
{
    public static class VocabularyStore
    {
        private static readonly string fileName = "vocab.json";

        public static Vocabulary Load()
        {
            var file = new FileInfo(fileName);
            if (!file.Exists)
            {
                Console.WriteLine($"new vocabulary created");
                return new Vocabulary();
            }

            var json = File.ReadAllText(file.FullName);
            return JsonSerializer.Deserialize<Vocabulary>(json)!;
        }

        public static void Save(Vocabulary vocabulary)
        {
            var json = JsonSerializer.Serialize(vocabulary, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(fileName, json);
        }
    }
}
