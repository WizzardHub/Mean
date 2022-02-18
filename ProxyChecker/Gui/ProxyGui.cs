using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Colorful;
using ProxyChecker.Models;
using ProxyChecker.Utils;
using Console = Colorful.Console;

namespace ProxyChecker
{
    public class ProxyGui
    {
        public Task UpdateGui(List<ProxyResponse> proxyResponses)
        {
            Console.Out.FlushAsync();
            int good, bad, countries;
            lock (proxyResponses)
            {
                good = proxyResponses
                    .FindAll(r => r.IsOk)
                    .Count;

                bad = proxyResponses.Count - good;

                countries = Math.Max(0, proxyResponses
                    .Select(x => x.Country?.IsoCode)
                    .Distinct()
                    .Count() - 1);
            }

            if (Console.CursorTop > 7)
                ConsoleUtils.ClearLines(2);

            var valid = new[]
            {
                new Formatter(good.ToString(), Color.PaleTurquoise),
                new Formatter("Valid Proxies", Color.Aquamarine),
                new Formatter("|", Color.Gray),
                new Formatter(countries.ToString(), Color.Plum),
                new Formatter("Countries", Color.PaleVioletRed)
            };
            
            var invalid = new[]
            {
                new Formatter(bad.ToString(), Color.HotPink),
                new Formatter("Invalid Proxies", Color.DeepPink)
            };
            
            ConsoleUtils.WriteLineCentered("[{0}] {1} {2} [{3}] {4}", valid);
            ConsoleUtils.WriteLineCentered("[{0}] {1}", invalid);

            return Task.CompletedTask;
        }

        public Task UpdateTitle(List<ProxyResponse> proxyResponses)
        {
            int good, bad;
            lock (proxyResponses)
            {
                good = proxyResponses
                    .FindAll(r => r.IsOk)
                    .Count;

                bad = proxyResponses.Count - good;
            }
            
            Console.Title =
                $"Mean | Valid [{good}] - Invalid [{bad}]";
            return Task.CompletedTask;
        }
    }
}