using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Colorful;
using Console = Colorful.Console;

namespace ProxyChecker.Utils
{

    public class ConsoleUtils
    {
        
        private const int FixedWidthTrueType = 54;
        private const int StandardOutputHandle = -11;

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetStdHandle(int nStdHandle);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool SetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool MaximumWindow, ref FontInfo ConsoleCurrentFontEx);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool GetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool MaximumWindow, ref FontInfo ConsoleCurrentFontEx);


        private static readonly IntPtr ConsoleOutputHandle = GetStdHandle(StandardOutputHandle);

        
        [DllImport("shlwapi.dll")]
        public static extern int ColorHLSToRGB(int h, int l, int s);
        
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct FontInfo
        {
            internal int cbSize;
            internal int FontIndex;
            internal short FontWidth;
            public short FontSize;
            public int FontFamily;
            public int FontWeight;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            //[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.wc, SizeConst = 32)]
            public string FontName;
        }

        public static void Log(LogType type, string message)
        {
            string format = default;
            Formatter[] formatters = default;
            
            switch (type)
            {
                case LogType.Info:
                    format = "[{0}] {1}";
                    formatters = new[]
                    {
                        new Formatter("@", Color.PaleGreen),
                        new Formatter(message, Color.PaleGreen)
                    };
                    break;
                case LogType.Warning:
                    format = "[{0}] {1}";
                    formatters = new[]
                    {
                        new Formatter("$", Color.PaleGoldenrod),
                        new Formatter(message, Color.PaleGoldenrod)
                    };
                    break;
                case LogType.Error:
                    format = "[{0}] {1}";
                    formatters = new[]
                    {
                        new Formatter("!", Color.PaleVioletRed),
                        new Formatter(message, Color.PaleVioletRed)
                    };
                    break;
            }
            
            Console.WriteLineFormatted(format, Color.DimGray, formatters);
        }
        
        public static FontInfo[] SetCurrentFont(string font, short fontSize = 0)
        {

            FontInfo before = new FontInfo
            {
                cbSize = Marshal.SizeOf<FontInfo>()
            };

            if (GetCurrentConsoleFontEx(ConsoleOutputHandle, false, ref before))
            {

                FontInfo set = new FontInfo
                {
                    cbSize = Marshal.SizeOf<FontInfo>(),
                    FontIndex = 0,
                    FontFamily = FixedWidthTrueType,
                    FontName = font,
                    FontWeight = 400,
                    FontSize = fontSize > 0 ? fontSize : before.FontSize
                };

                // Get some settings from current font.
                if (!SetCurrentConsoleFontEx(ConsoleOutputHandle, false, ref set))
                {
                    var ex = Marshal.GetLastWin32Error();
                    Console.WriteLine("Set error " + ex);
                    throw new Win32Exception(ex);
                }

                FontInfo after = new FontInfo
                {
                    cbSize = Marshal.SizeOf<FontInfo>()
                };
                GetCurrentConsoleFontEx(ConsoleOutputHandle, false, ref after);

                return new[] { before, set, after };
            }
            else
            {
                var er = Marshal.GetLastWin32Error();
                Console.WriteLine("Get error " + er);
                throw new Win32Exception(er);
            }
        }

        // Logo
        
        public static void WriteLogo()
        {
            var lines = new[]
            {
                "",
                @"  /\/\   ___  __ _ _ __  ",
                @" /    \ / _ \/ _` | '_ \ ",
                @"/ /\/\ \  __/ (_| | | | |",
                @"\/    \/\___|\__,_|_| |_|",
                ""
            };
            
            int buffer = 0;
            foreach (var line in lines)
            {
                int r = 120 + buffer, g = 30 + buffer, b = 80 + buffer;
                for (int i = 0; i < line.Length; i++)
                {
                    var color = Color.FromArgb(r, g, b);
                    WriteCentered(line[i].ToString(), color, i + 3, 4);
                }
                Console.Out.WriteLineAsync();
                buffer += 20;
            }
        }

        public static void ClearLine()
        {
            Console.Out.FlushAsync();
            Console.CursorTop = Math.Max(0, Console.CursorTop - 1);
            Console.CursorLeft = 0;
            Console.Out.WriteAsync(new string(' ', Console.WindowWidth));
            Console.CursorTop = Math.Max(0, Console.CursorTop - 1);
        }

        public static void ClearLines(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                ClearLine();
            }
        }

        public static void Clear()
        {
            Console.CursorTop = 0;
            Console.Out.WriteLineAsync(new string(' ', Console.WindowWidth * Console.WindowHeight));
            Console.CursorTop = 0;
            
            Console.Out.FlushAsync();
        }
        
        public static void WriteLineCentered(string value, Color color = default)
        {
            Console.CursorLeft = Console.WindowWidth / 2 - value.Length / 2;
            Console.WriteLine(value, color);
            Console.CursorLeft = 0;
        }
        
        public static void WriteLineCentered(string value, Formatter[] formatters)
        {
            
            int formatterLength = formatters
                .ToList()
                .Select(x => x
                    .Target
                    .ToString()
                    .Length)
                .Sum();

            int targetLength = value
                .Length - value
                .ToList()
                .FindAll(x => x.Equals('{'))
                .Count * 3;

            Console.CursorLeft = Console.WindowWidth / 2 - targetLength / 2 - formatterLength / 2;
            Console.WriteLineFormatted(value, Color.Gray, formatters);
            Console.CursorLeft = 0;
        }
        
        public static void WriteCentered(string value, Color color = default, int index = default, int factor = 2)
        {
            Console.CursorLeft = Console.WindowWidth / factor - value.Length / factor + index;
            Console.Write(value, color);
            Console.CursorLeft = 0;
        }
        
    }

    public enum LogType
    {
        Info,
        Warning,
        Error
    }
}