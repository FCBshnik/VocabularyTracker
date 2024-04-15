namespace VocabTrack.Core
{
    public class SubsParser
    {
        public List<string> Parse(FileInfo fileInfo)
        {
            using (var stream = fileInfo.OpenRead())
                return Parse(stream);
        }

        public List<string> Parse(Stream textStream)
        {
            var parser = new SubtitlesParser.Classes.Parsers.SubParser();
            var items = parser.ParseStream(textStream);
            return items.SelectMany(i => i.Lines).ToList();
        }
    }
}
