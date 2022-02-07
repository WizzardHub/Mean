using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Leaf.xNet;
using MaxMind.GeoIP2;
using ProxyChecker.Extensions;
using ProxyChecker.Models;
using ProxyChecker.Utils;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ProxyChecker
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            #region Pre Init

            Console.WindowWidth = 80;
            Console.OutputEncoding = Encoding.Unicode;
            Console.BufferHeight = Console.WindowHeight;
            Console.BufferWidth = Console.WindowWidth;
            Console.CursorVisible = false;
            Console.Title = "";
            
            ConsoleUtils.SetCurrentFont("Consolas", 16);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance) // naming convention using capital letters
                .Build();

            #endregion

            #region Config Loading

            if (!File.Exists("config.yml"))
            {
                // Automatically disposes object after used
                using (var resource = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("ProxyChecker.Resources.config.yml"))
                using (var file = new FileStream("config.yml", FileMode.Create, FileAccess.Write))
                {
                    resource.CopyTo(file);
                }
            }
            
            var yml = File.OpenText("config.yml");
            var config = deserializer.Deserialize<Config>(yml);

            #endregion

            #region Post Init

            // Looks like we're properly setting up the thread pool
            // By default the maximum threads amount is equal to your cpu's threads
            // And it's quite retarded to pretend having multi-threaded shit without setting up the threadpool
            ThreadPool.GetAvailableThreads(out int workerThreads, out int completionPortThreads);
            ThreadPool.SetMinThreads(config.Checker.Threads + 10, completionPortThreads);

            #endregion
            
            #region Load Proxies

            ConsoleUtils.Log(LogType.Info, "Load your proxies ...");
            
            var proxies = File
                .ReadLines(FileDialogUtils.SelectFile())
                .ToList();

            ConsoleUtils.Log(LogType.Info, $"Successfully loaded {proxies.Count} proxies !");
            
            #endregion

            #region Checking Proxy

            Parallel.ForEach(proxies, new ParallelOptions { MaxDegreeOfParallelism = config.Checker.Threads }, proxy =>
            {
                try
                {
                    using (var req = new HttpRequest()
                        .UseProxy(proxy, config.Proxy.Type, config.Proxy.TimeOut))
                    {
                        var res = req.Get("http://www.proxy-listen.de/azenv.php");
                        if (res.IsOK)
                        {
                            string country;
                            using (var reader = new DatabaseReader("GeoLite2.mmdb"))
                            {
                                var response = reader.Country(proxy.Split(':')[0]);
                                country = response.Country.Name;
                            }
                            
                            ConsoleUtils.Log(LogType.Info, $"{proxy} ({country})");
                        }
                    }
                }
                catch (HttpException e)
                {
                    ConsoleUtils.Log(LogType.Warning, $"{proxy} (Timed Out)");
                }
            });

            #endregion

            Thread.Sleep(-1);
        }
    }
}