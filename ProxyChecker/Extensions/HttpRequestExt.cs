using System;
using Leaf.xNet;

namespace ProxyChecker.Extensions
{
    public static class HttpRequestExt
    {
        
        /*
         * Returns a proxied HttpRequest with adequate parameters :
         * Disables cookies
         * Uses keepalive if necessary
         * Defines timeouts
         */
        
        public static HttpRequest UseProxy(this HttpRequest req, string proxy, ProxyType proxyType, bool keepAlive, int timeout)
        {
            
            req.Proxy = proxyType switch
            {
                ProxyType.HTTP => HttpProxyClient.Parse(proxy),
                ProxyType.Socks4 => Socks4ProxyClient.Parse(proxy),
                ProxyType.Socks4A => Socks4ProxyClient.Parse(proxy),
                ProxyType.Socks5 => Socks4ProxyClient.Parse(proxy)
            };
            
            req.UseCookies = false;
            req.KeepAlive = keepAlive;
            req.AllowAutoRedirect = false;
            req.MaximumKeepAliveRequests = Int32.MaxValue;

            req.ConnectTimeout = timeout;
            req.KeepAliveTimeout = timeout / 4;
            req.ReadWriteTimeout = timeout / 4;
            
            return req;
        }
    }
}