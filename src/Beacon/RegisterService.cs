using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beacon
{
    public class RegisterService
    {
        public RegisterService(string name, string url)
        {
            Url = url;
            Name = name;
        }

        public string Name { get;private set; }
        public string Url { get;private set; }
    }
}
