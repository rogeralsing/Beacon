namespace Shared
{
    public class RegisterService
    {
        public RegisterService(string name, string url)
        {
            Url = url;
            Name = name;
        }

        public string Name { get; private set; }
        public string Url { get; private set; }
    }
}