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
            var mode = Prompt.Select<Mode>("Select mode");
            if (mode == Mode.AddSubs)
                AddNewWords(vocab);
            else if (mode == Mode.ListTodo)
                Console.WriteLine(string.Join(Environment.NewLine, vocab.Todo.ListWords()));
            else if (mode == Mode.ManageTodo)
                ManageTodo(vocab);
            else
                break;
        }

        VocabularyStore.Save(vocab);
    }

    private static void ManageTodo(Vocabulary vocab)
    {
        foreach (var word in vocab.Todo.ListWords())
        {
            var wordAction = Prompt.Select<TodoWordAction>($"Select action for '{word}'", defaultValue: TodoWordAction.Skip);
            if (wordAction == TodoWordAction.Learn)
            {
                vocab.Todo.RemoveWord(word);
                vocab.Learned.AddWord(word);
            }
            if (wordAction == TodoWordAction.Break)
                return;
        }
    }

    private static void AddNewWords(Vocabulary vocab)
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

            Console.WriteLine($"{word} - {pair.Value}");

            var wordAction = Prompt.Select<NewWordAction>($"Select action for new word '{word}'", defaultValue: NewWordAction.Add);
            if (wordAction == NewWordAction.Add)
                vocab.Learned.AddWord(word);
            else if (wordAction == NewWordAction.Todo)
                vocab.Todo.AddWord(word);
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
        [Display(Name = "List todo words")]
        ListTodo,
        [Display(Name = "Manage todo words")]
        ManageTodo,
        [Display(Name = "Exit")]
        Exit,
    }

    private enum NewWordAction
    {
        [Display(Name = "Add as learned")]
        Add,
        [Display(Name = "Add as todo")]
        Todo,
        [Display(Name = "Add as name")]
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
