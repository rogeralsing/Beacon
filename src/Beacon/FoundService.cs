namespace Beacon
{
    public class FoundService
    {
        public FoundService(Service service)
        {
            Service = service;
        }

        public Service Service { get;private set; }
    }
}
