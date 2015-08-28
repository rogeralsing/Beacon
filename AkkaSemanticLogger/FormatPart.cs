using Sprache;

namespace AkkaSemanticLogger
{
    public class FormatPart
    {
        public FormatPart(string text, IOption<StringFormat> format)
        {
            Text = text;
            Format = format;
        }

        public string Text { get; set; }

        public IOption<StringFormat> Format { get; set; }
    }
}