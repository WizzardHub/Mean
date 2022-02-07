using System;
using System.Drawing;
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
                    throw new System.ComponentModel.Win32Exception(ex);
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
                throw new System.ComponentModel.Win32Exception(er);
            }
        }
        
    }

    public enum LogType
    {
        Info,
        Warning,
        Error
    }
}