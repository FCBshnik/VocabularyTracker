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
            Console.WriteLine($"Vocabulary: total words {vocab.Words.ListWords().Count}");
            var mode = Prompt.Select<Mode>("Select mode");
            if (mode == Mode.AddSubs)
                AddSubs(vocab);
            else if (mode == Mode.StudyWords)
                StudyWords(vocab);
            else if (mode == Mode.ShowStats)
                ShowStats(vocab);
            else
                break;
        }

        VocabularyStore.Save(vocab);
    }

    private static void ShowStats(Vocabulary vocab)
    {
        var words = vocab.Words.ListWords();
        var learnedWordsCount = words.Count(w => w.Value.Note == WordMark.Learned);
        var namesWordsCount = words.Count(w => w.Value.Note == WordMark.Name);
        var newWords = words.Where(w => w.Value.Note == null)
            .OrderByDescending(w => w.Value.Occurrences)
            .ThenBy(w => w.Key)
            .ToList();

        Console.WriteLine($"Vocabulary contains words:");
        Console.WriteLine($"\ttotal {words.Count}");
        Console.WriteLine($"\tlearned {learnedWordsCount}");
        Console.WriteLine($"\tnames {namesWordsCount}");
        Console.WriteLine($"\tnew {newWords.Count}");

        Console.WriteLine($"Top 20 most used words:");
        var mostUsed = words.OrderByDescending(w => w.Value.Occurrences).Take(20).ToList();
        foreach(var word in mostUsed)
            Console.WriteLine($"\t'{word.Key} - {word.Value.Occurrences} occurrences");
    }

    private static void StudyWords(Vocabulary vocab)
    {
        var words = vocab.Words.ListWords();
        var newWords = words.Where(w => w.Value.Note == null)
            .OrderByDescending(w => w.Value.UpdatedAt)
            .ThenByDescending(w => w.Value.Occurrences)
            .ThenBy(w => w.Key)
            .ToList();

        Console.WriteLine($"Vocabulary contains {newWords.Count} words to study");

        Console.WriteLine($"Top 10 latest words to study:");
        foreach (var word in newWords.Take(10))
            Console.WriteLine($"\t'{word.Key} - {word.Value.Occurrences} occurrences");

        foreach (var pair in newWords)
        {
            var word = pair.Key;
            var info = pair.Value;

            var wordAction = Prompt.Select<WordAction>($"Select action for '{word}' ({info.Occurrences} occurrences)", defaultValue: WordAction.Skip);
            if (wordAction == WordAction.Back)
                break;
            if (wordAction == WordAction.Skip)
                continue;
            if (wordAction == WordAction.Learn)
                vocab.Words.SetWordNote(word, WordMark.Learned);
            else if (wordAction == WordAction.Name)
                vocab.Words.SetWordNote(word, WordMark.Name);
        }
    }

    private static void AddSubs(Vocabulary vocab)
    {
        var srtFiles = new DirectoryInfo("./").EnumerateFiles("*.srt", SearchOption.AllDirectories).ToList();
        var srtFile = Prompt.Select("Select subtitles file from app directory", srtFiles, textSelector: f => f.Name);
        var srtName = srtFile.Name;
        if (!srtFile.Exists)
        {
            Console.WriteLine($"File '{srtFile.FullName}' does not exist");
            return;
        }

        var time = DateTime.UtcNow;
        var subsParser = new SubsParser();
        var lines = subsParser.Parse(srtFile);
        var subsWords = new WordsExtractor().ExtractWords(lines).OrderByDescending(p => p.Value).ThenBy(p => p.Key).ToList();
        var vocabWords = vocab.Words.ListWords();

        if (!vocab.Subs.ContainsKey(srtName))
        {
            var newWords = subsWords.Where(w => !vocabWords.ContainsKey(w.Key)).ToList();
            var existingWordsCount = subsWords.Count(w => vocabWords.ContainsKey(w.Key));

            foreach (var pair in subsWords)
                vocab.Words.IncrementWordOccurrences(pair.Key, time, pair.Value);

            vocab.Subs[srtName] = DateTime.UtcNow.ToString();

            Console.WriteLine($"Subtitles '{srtName}' contains: {lines.Count} lines, {subsWords.Count} words");
            Console.WriteLine($"Added occurences for {subsWords.Count} unique words");
            Console.WriteLine($"Added {newWords.Count} new words");
            Console.WriteLine($"Skipped {existingWordsCount} already added words");
            Console.WriteLine($"Top 10 new words:");
            foreach (var word in newWords.OrderByDescending(w => w.Value).Take(10))
                Console.WriteLine($"\t{word.Key} - {word.Value} occurences");
        }
        else
        {
            Console.WriteLine($"Subtitles '{srtName}' have already been added");
        }
    }

    private static class WordMark
    {
        public const string Learned = "l";
        public const string Name = "n";
    }

    private enum Mode
    {
        [Display(Name = "Add new subtitles file")]
        AddSubs,
        [Display(Name = "Study words")]
        StudyWords,
        [Display(Name = "Show stats")]
        ShowStats,
        [Display(Name = "Exit")]
        Exit,
    }

    private enum WordAction
    {
        [Display(Name = "Skip")]
        Skip,
        [Display(Name = "Mark as learned")]
        Learn,
        [Display(Name = "Mark as name")]
        Name,
        [Display(Name = "Back")]
        Back,
    }
}
