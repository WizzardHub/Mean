using MaxMind.GeoIP2.Model;

namespace ProxyChecker.Models
{
    public class ProxyResponse
    {

        private bool _isOk;
        private string _proxy;
        private Country _country;

        public bool IsOk
        {
            get => _isOk;
            set => _isOk = value;
        }
        
        public string Proxy
        {
            get => _proxy;
            set => _proxy = value;
        }

        public Country Country
        {
            get => _country;
            set => _country = value;
        }

        public ProxyResponse(bool isOk, string proxy, Country country = default)
        {
            _isOk = isOk;
            _proxy = proxy;
            _country = country;
        }
    }
}