﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Krawlr.Core
{
    public interface ILog
    {
        void WriteLine(string s, ConsoleColor color);
        void Debug(string s);
        void Exclaim(string s);
        void Info(string s);
        void Warn(string s);
        void Error(string s);
    }

    /// <summary>
    /// Really simple console logging.
    /// </summary>
    public class Log : ILog
    {
        public void WriteLine(string s, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(s);
            Console.ResetColor();

#if DEBUG
            System.Diagnostics.Debug.WriteLine(s, "Output");
#endif
        }

        public void Debug(string s)
        {
            WriteLine(s, ConsoleColor.DarkGray);
        }

        public void Exclaim(string s)
        {
            WriteLine(s, ConsoleColor.Cyan);
        }

        public void Info(string s)
        {
            WriteLine(s, ConsoleColor.Gray);
        }

        public void Warn(string s)
        {
            WriteLine(s, ConsoleColor.Yellow);
        }

        public void Error(string s)
        {
            WriteLine(s, ConsoleColor.Red);
        }
    }
}
