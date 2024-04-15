using VocabTrack.Core;

namespace VocabTrack.Cli;

public static class Program
{
    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.Unicode;

        var subsParser = new SubsParser();
        var lines = subsParser.Parse(new FileInfo(@"C:\@yan\share\video\The.Tourist.S01E05.WEB-DLRip.RGzsRutracker.[Wentworth_Miller].srt"));

        Console.WriteLine($"lines:");
        foreach (var line in lines.Take(10))
        {
            Console.WriteLine(line);
        }

        var vocab = VocabularyStore.Load();

        var words = new WordsExtractor().ExtractWords(lines);
        Console.WriteLine($"\r\nwords {words.Count}:");
        foreach (var pair in words.Where(w => !vocab.ContainsWord(w.Key)).OrderByDescending(p => p.Value).Take(10))
        {
            var word = pair.Key;

            Console.WriteLine($"{word} - {pair.Value}");

            var option = Console.ReadLine();
            switch (option)
            {
                case "a":
                    vocab.Learned.Add(word);
                    Console.WriteLine($"added as learned");
                    break;
                case "n":
                    vocab.Names.Add(word);
                    Console.WriteLine($"added as name");
                    break;
                case "t":
                    vocab.Todo.Add(word);
                    Console.WriteLine($"added as todo");
                    break;
                case "s":
                    break;
                default:
                    Console.WriteLine($"invalid option '{option}'");
                    break;
            }
        }

        Console.WriteLine($"save vocab:");
        VocabularyStore.Save(vocab);
    }
}
