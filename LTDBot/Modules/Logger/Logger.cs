using System;
using System.IO;

namespace LTDBot.Modules.Logger
{
    public class Logger
    {
        private static Logger _instance;
        private static string File => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Logs\logger.log");

        public static Logger GetInstance()
        {
            return _instance ?? (_instance = new Logger());
        }

        public void Log(string text)
        {
            System.IO.File.AppendAllText(File, Decorate(text));
        }

        private string Decorate(string text)
        {
            return $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: {text}{Environment.NewLine}{Environment.NewLine}";
        }
    }
}