using System.Text.Json;

namespace VocabTrack.Core
{
    public static class VocabularyStore
    {
        private static readonly string filePath = "db/vocab.json";

        public static Vocabulary Load()
        {
            var file = new FileInfo(filePath);
            if (!file.Exists)
            {
                Console.WriteLine($"New vocabulary was created at {file.FullName}");
                var vocab = new Vocabulary();
                Save(vocab);
                return vocab;
            }

            var json = File.ReadAllText(file.FullName);
            Console.WriteLine($"Vocabulary loaded from {file.FullName}");
            return JsonSerializer.Deserialize<Vocabulary>(json)!;
        }

        public static void Save(Vocabulary vocabulary)
        {
            var file = new FileInfo(filePath);
            if(!file.Directory!.Exists)
                file.Directory.Create();

            var json = JsonSerializer.Serialize(vocabulary, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(file.FullName, json);
        }
    }
}
