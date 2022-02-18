using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Leaf.xNet;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Model;
using ProxyChecker.Extensions;
using ProxyChecker.Models;
using ProxyChecker.Utils;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ProxyChecker
{
    internal class Mean
    {
        
        private static List<ProxyResponse> 
            _proxyResponses = new(),
            _proxySavePool = new();
        
        [STAThread]
        public static void Main(string[] args)
        {
            #region Pre Init

            Console.WindowWidth = 60;
            Console.WindowHeight = 10;
            Console.OutputEncoding = Encoding.Unicode;
            Console.BufferHeight = Console.WindowHeight;
            Console.BufferWidth = Console.WindowWidth;
            Console.CursorVisible = false;
            Console.Title = "";

            bool isStarted = false;
            
            ConsoleUtils.SetCurrentFont("Consolas", 20);
            ConsoleUtils.WriteLogo();

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
            ThreadPool.SetMinThreads(config.Checker.Threads, 0);
            
            Task.Run(() =>
            {
                var gui = new ProxyGui();
                for (;;)
                {
                    if (!isStarted)
                        continue;

                    Thread.Sleep(250);

                    try
                    {
                        Parallel.Invoke(
                            () => gui.UpdateGui(_proxyResponses),
                            () => gui.UpdateTitle(_proxyResponses),
                            () => FileUtils.SaveProxy(_proxySavePool));
                    }
                    catch { }
                    
                    lock (_proxySavePool)
                        _proxySavePool.Clear();
                }
            });
            
            #endregion
            
            #region Load Proxies

            ConsoleUtils.Log(LogType.Info, "Load your proxies ...");
            
            var proxies = File
                .ReadLines(FileDialogUtils.SelectFile())
                .ToList();

            ConsoleUtils.Log(LogType.Info, $"Successfully loaded {proxies.Count} proxies !");
            Thread.Sleep(1000);

            ConsoleUtils.ClearLines(2);
            isStarted = true;

            #endregion

            #region Checking Proxy

            proxies
                .Distinct()
                .AsParallel()
                .WithDegreeOfParallelism(config.Checker.Threads)
                .WithMergeOptions(ParallelMergeOptions.NotBuffered)
                .ForAll(proxy =>
                {
                    using (var req = new HttpRequest()
                        .UseProxy(proxy, config.Proxy.Type, config.Checker.KeepAlive, config.Proxy.TimeOut))
                    {
                        try
                        {
                            var res = req.Get("https://www.proxy-listen.de/azenv.php");
                            var regex = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
                            
                            var matches = regex
                                .IsValidMatch(res.ToString());
                            
                            if (res.IsOK && matches)
                            {
                                Country country;
                                var ipAddress = regex
                                    .MatchFirst(res.ToString())
                                    .Value;

                                using (var reader = new DatabaseReader("GeoLite2.mmdb"))
                                {
                                    var response = reader.Country(ipAddress);
                                    country = response.Country;
                                }

                                lock (_proxyResponses)
                                    _proxyResponses.Add(new ProxyResponse(true, proxy, country));
                                
                                lock (_proxySavePool)
                                    _proxySavePool.Add(new ProxyResponse(true, proxy, country));
                            }
                            else
                            {
                                lock (_proxyResponses)
                                    _proxyResponses.Add(new ProxyResponse(false, proxy));
                            }
                        }
                        catch (HttpException e)
                        {
                            lock (_proxyResponses)
                                _proxyResponses.Add(new ProxyResponse(false, proxy));
                        }
                        finally
                        {
                            req.Close();
                        }
                    }
                });

            #endregion
            
            Thread.Sleep(-1);
        }
    }
}