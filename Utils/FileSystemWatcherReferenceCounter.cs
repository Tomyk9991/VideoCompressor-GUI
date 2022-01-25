using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Documents;

namespace VideoCompressorGUI.Utils
{
    public class FileSystemWatcherReferenceCounter
    {
        public FileSystemWatcher Watcher { get; set; }
        public int ReferenceCount => files.Count;

        private List<string> files = new(); 

        public FileSystemWatcherReferenceCounter(FileSystemWatcher watcher, string file)
        {
            Watcher = watcher;
            this.files.Add(file);
        }

        public int Decrease(string file)
        {
            this.files.Remove(file);
            return ReferenceCount;
        }

        public int Increase(string file)
        {
            this.files.Add(file);
            return ReferenceCount;
        }
    }
}