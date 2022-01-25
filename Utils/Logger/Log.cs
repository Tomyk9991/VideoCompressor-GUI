using System;
using System.IO;
using System.Text;

namespace VideoCompressorGUI.Utils.Logger
{
    public class Log
    {
        private object mutex = new();
        private string relativePath;
        private string fullPath;
        private static Log instance;
        
        
        public bool LogToConsole { get; set; }
        public bool LogToFile { get; set; }

        public string RelativePath
        {
            get => relativePath;
            set
            {
                relativePath = value;
                fullPath = Path.GetDirectoryName(typeof(Log).Assembly.Location) + RelativePath;
                Directory.CreateDirectory(fullPath);
            }
        }
        public DateTime ApplicationStarted { get; private set; }

        public Log()
        {
            this.LogToConsole = true;
            this.LogToFile = true;
            this.RelativePath = "\\logs\\";
            instance = this;
            
            this.ApplicationStarted = DateTime.Now;
            
            
        }

        public void WarnPrint(string message)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("======= WARN: ==").Append(DateTime.Now.ToShortTimeString()).Append("==============\n")
                .AppendLine(message);
            
            lock (mutex)
            {
                if (LogToConsole)
                    Console.WriteLine(message);
                
                if (LogToFile && fullPath != "")
                    WriteFile(builder.ToString());
            }
        }

        public static void Warn(string message)
            => instance.WarnPrint(message);
        
        public static void Info(string message)
            => instance.InfoPrint(message);
        
        public void InfoPrint(string message)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("======= INFO: ==").Append(DateTime.Now.ToShortTimeString()).Append("==============\n")
                .AppendLine(message);
            
            lock (mutex)
            {
                if (LogToConsole)
                    Console.WriteLine(message);
                
                if (LogToFile && fullPath != "")
                    WriteFile(builder.ToString());
            }
        }

        public void Error(Exception ex, bool logToConsole = false)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("====== ERROR: ==").Append(DateTime.Now.ToShortTimeString()).Append("==============\n");
            builder.Append("Message:\n\n").Append(ex.Message);
            builder.Append("\nInner message:\n\n").Append(ex.InnerException);
            builder.Append("\nStack trace:\n\n").Append(ex.StackTrace);
            builder.Append("\n\n\n\n");

            string bu = builder.ToString();

            lock (mutex)
            {
                if (LogToConsole && logToConsole)
                    Console.WriteLine(bu);


                if (LogToFile && fullPath != "")
                    WriteFile(bu);
            }
        }

        private void WriteFile(string message)
        {
            string logPath = fullPath + ApplicationStarted.ToFileTime()  + ".log";
            using StreamWriter sw = File.AppendText(logPath);
            sw.WriteLine(message);
            sw.Close();
        }
    }
}