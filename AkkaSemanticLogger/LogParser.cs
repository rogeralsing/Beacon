using System.Collections.Generic;
using Sprache;

namespace AkkaSemanticLogger
{
    public static class LogParser
    {
        public static Parser<StringFormat> StringFormat =
            from _1 in Parse.String("{")
            from index in Parse.Number
            from _2 in Parse.String("}")
            select new StringFormat(index);

        public static Parser<string> AnyText = Parse.AnyChar.Except(Parse.Char('{')).Many().Text();

        public static Parser<FormatPart> Part =
            from text in AnyText
            from format in StringFormat.Optional()
            select new FormatPart(text, format);


        public static Parser<IEnumerable<FormatPart>> Parts = Part.Many();
    }
}