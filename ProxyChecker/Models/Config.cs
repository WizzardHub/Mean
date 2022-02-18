using Leaf.xNet;

namespace ProxyChecker.Models
{
    public class Config
    {
        public Proxy Proxy { get; private set; }
        public Checker Checker { get; private set; }
    }

    public class Proxy
    {
        public ProxyType Type { get; private set; }
        public int TimeOut { get; private set; }
    }

    public class Checker
    {
        public int Threads { get; private set; }
        public bool KeepAlive { get; private set; }
    }
}