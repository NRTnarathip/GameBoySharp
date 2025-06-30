using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoySharp
{
    public enum LogLevel
    {
        Info,
    }
    public static class Logger
    {
        public static void Log(params object[] args)
        {
            Info(args);
        }
        public static void Info(params object[] args)
        {
            Console.WriteLine($"{TimeStamp} [INFO] {MakeArgsLine(args)}");
        }
        static string TimeStamp => GetTimeStamp();
        static Stopwatch GlobalTimer = Stopwatch.StartNew();

        static string GetTimeStamp()
        {
            return $"[{GlobalTimer.Elapsed.TotalSeconds:F5}s]";
        }

        static string MakeArgsLine(object[] args)
        {
            List<string> items = new();
            foreach (var arg in args)
            {
                items.Add(arg.ToString());
            }
            return string.Join(" ", items.ToArray());
        }
    }
}
