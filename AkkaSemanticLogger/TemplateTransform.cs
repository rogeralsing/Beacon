using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Text;
using Sprache;

namespace AkkaSemanticLogger
{
    class TemplateTransform
    {
        private static readonly TextInfo TextInfo = new CultureInfo("en-US", false).TextInfo;
        private static readonly ConcurrentDictionary<string, string> Templates = new ConcurrentDictionary<string, string>();
        public static string Transform(string template)
        {
            //template is not of old format
            if (!template.Contains("{0"))
                return template;

            string transformed;

            if (!Templates.TryGetValue(template, out transformed))
            {
                try
                {
                    var unknownIndex = 0;
                    var parts = LogParser.Parts.Parse(template).ToArray();

                    var sb = new StringBuilder();
                    foreach (var part in parts)
                    {
                        sb.Append(part.Text);
                        if (part.Format.IsDefined)
                        {
                            sb.Append("{");
                            var cleaned = TextInfo.ToTitleCase(part.Text);
                            var x = (from c in cleaned
                                     where char.IsLetterOrDigit(c)
                                     select c).ToArray();
                            var formatName = new string(x);
                            if (string.IsNullOrEmpty(formatName))
                            {
                                formatName = "Obj" + unknownIndex++;
                            }
                            sb.Append(formatName);
                            sb.Append("}");
                        }
                    }
                    transformed = sb.ToString();
                    Templates.TryAdd(template, transformed);
                }
                catch
                {
                    //transform broke, fall back to original
                    transformed = template;
                    Templates.TryAdd(template, transformed);
                }
            }

            return transformed;
        }
    }
}
