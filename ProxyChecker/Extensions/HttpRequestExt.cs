using System;
using Leaf.xNet;

namespace ProxyChecker.Extensions
{
    public static class HttpRequestExt
    {
        public static HttpRequest UseProxy(this HttpRequest req, string proxy, ProxyType proxyType, int timeout)
        {
            req.Proxy = proxyType switch
            {
                ProxyType.HTTP => HttpProxyClient.Parse(proxy),
                ProxyType.Socks4 => Socks4ProxyClient.Parse(proxy),
                ProxyType.Socks4A => Socks4ProxyClient.Parse(proxy),
                ProxyType.Socks5 => Socks4ProxyClient.Parse(proxy)
            };
            req.IgnoreProtocolErrors = true;
            req.ConnectTimeout = timeout;
            req.KeepAliveTimeout = timeout;
            req.ReadWriteTimeout = timeout;
            return req;
        }
    }
}