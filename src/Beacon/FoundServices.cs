using System.Collections.Generic;
using System.Linq;

namespace Beacon
{
    public class FoundServices
    {
        public FoundServices(IEnumerable<Service> services)
        {
            Services = services.ToArray();
        }
        public Service[] Services { get;private set; }
    }

    public class Service
    {
        private readonly HashSet<string> _urls;
        public Service(string name, IEnumerable<string> urls)
        {
            Name = name;
            _urls = urls != null ? new HashSet<string>(urls) : new HashSet<string>();
        }

        public string Name { get;private set; }

        public void AddUrl(string url)
        {
            _urls.Add(url);
        }

        public string[] URLs
        {
            get { return _urls.ToArray(); }
        }
    }
}
