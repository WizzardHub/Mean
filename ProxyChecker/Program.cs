using System;
using System.IO;
using System.Reflection;
using System.Threading;
using ProxyChecker.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ProxyChecker
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            #region Pre Init

            Console.WindowWidth = 80;
            Console.BufferHeight = Console.WindowHeight;
            Console.BufferWidth = Console.WindowWidth;
            Console.CursorVisible = false;
            Console.Title = "";
            
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
            ThreadPool.GetAvailableThreads(out int workerThreads, out int completionPortThreads);
            ThreadPool.SetMinThreads(config.Checker.Threads + 10, completionPortThreads);

            #endregion

            Thread.Sleep(-1);
        }
    }
}