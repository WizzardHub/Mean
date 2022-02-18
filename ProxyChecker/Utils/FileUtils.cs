using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ProxyChecker.Models;

namespace ProxyChecker.Utils
{
    public class FileUtils
    {
        public static Task SaveProxy(List<ProxyResponse> proxy)
        {
            
            if (proxy.Count <= 0)
                return Task.CompletedTask;

            var currentCheckPath = Process
                .GetCurrentProcess()
                .StartTime
                .ToString("R")
                .Replace(":", "-");

            if (!Directory.Exists($"Results\\{currentCheckPath}\\Countries"))
                Directory.CreateDirectory($"Results\\{currentCheckPath}\\Countries");

            proxy
                .ForEach(px =>
                {
                    
                    using (var stream = File.AppendText($"Results\\{currentCheckPath}\\Alive.txt"))
                        stream.WriteLineAsync(px?.Proxy);
                    
                    var code = px?.Country?.IsoCode;
                    using (var stream = File.AppendText($"Results\\{currentCheckPath}\\Countries\\{code}.txt"))
                        stream.WriteLineAsync(px?.Proxy);
                });
            
            return Task.CompletedTask;
        }
    }
}