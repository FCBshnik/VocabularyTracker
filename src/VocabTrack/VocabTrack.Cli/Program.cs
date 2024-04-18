using Sharprompt;
using System.ComponentModel.DataAnnotations;
using VocabTrack.Core;

namespace VocabTrack.Cli;

public static class Program
{
    public static void Main()
    {
        var vocab = VocabularyStore.Load();

        while (true)
        {
            Console.WriteLine($"Vocabulary: learned {vocab.Learned.ListWords().Count}, unknown {vocab.Unknown.ListWords().Count}");
            var mode = Prompt.Select<Mode>("Select mode");
            if (mode == Mode.AddSubs)
                AddSubs(vocab);
            else if (mode == Mode.ListUnknown)
                ListUnknown(vocab);
            else
                break;
        }

        VocabularyStore.Save(vocab);
    }

    private static void ListUnknown(Vocabulary vocab)
    {
        while (true)
        {
            var words = vocab.Unknown.ListWords();
            var word = Prompt.Select("Select word", words);
            var wordAction = Prompt.Select<TodoWordAction>($"Select action for '{word}'", defaultValue: TodoWordAction.Skip);
            if (wordAction == TodoWordAction.Learn)
            {
                vocab.Unknown.RemoveWord(word);
                vocab.Learned.AddWord(word);
            }
            if (wordAction == TodoWordAction.Break)
                break;
        }
    }

    private static void AddSubs(Vocabulary vocab)
    {
        var srtFiles = new DirectoryInfo("./").EnumerateFiles("*.srt", SearchOption.AllDirectories).ToList();
        var srtFile = Prompt.Select("Select subtitles file from app directory", srtFiles, textSelector: f => f.Name);
        if (!srtFile.Exists)
        {
            Console.WriteLine($"File '{srtFile.FullName}' does not exist");
            return;
        }

        var subsParser = new SubsParser();
        var lines = subsParser.Parse(srtFile);
        var words = new WordsExtractor().ExtractWords(lines);
        var newWords = words.Where(w => !vocab.ContainsWord(w.Key)).OrderByDescending(p => p.Value).ToList();
        Console.WriteLine($"Subtitles '{srtFile.FullName}' contains:");
        Console.WriteLine($"{lines.Count} text lines");
        Console.WriteLine($"{words.Count} unique words");
        Console.WriteLine($"{newWords.Count} new words");

        foreach (var pair in newWords)
        {
            var word = pair.Key;
            var wordAction = Prompt.Select<NewWordAction>($"Select action for new word '{word}' (occurs {pair.Value} times)", defaultValue: NewWordAction.Skip);
            if (wordAction == NewWordAction.Add)
                vocab.Learned.AddWord(word);
            else if (wordAction == NewWordAction.Todo)
                vocab.Unknown.AddWord(word);
            else if (wordAction == NewWordAction.Name)
                vocab.Names.AddWord(word);
            else if (wordAction == NewWordAction.Break)
                return;
        }
    }

    private enum Mode
    {
        [Display(Name = "Add new subtitles file")]
        AddSubs,
        [Display(Name = "List unknown words")]
        ListUnknown,
        [Display(Name = "Exit")]
        Exit,
    }

    private enum NewWordAction
    {
        [Display(Name = "Add to learned")]
        Add,
        [Display(Name = "Add to unknown")]
        Todo,
        [Display(Name = "Add to names")]
        Name,
        [Display(Name = "Skip")]
        Skip,
        [Display(Name = "Break")]
        Break,
    }

    private enum TodoWordAction
    {
        [Display(Name = "Skip")]
        Skip,
        [Display(Name = "Learn")]
        Learn,
        [Display(Name = "Break")]
        Break,
    }
}
