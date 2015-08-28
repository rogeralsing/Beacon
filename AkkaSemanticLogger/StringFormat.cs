namespace AkkaSemanticLogger
{
    public class StringFormat
    {
        public int Index { get; }
        public StringFormat(string index)
        {
            Index = int.Parse(index);
        }
    }
}