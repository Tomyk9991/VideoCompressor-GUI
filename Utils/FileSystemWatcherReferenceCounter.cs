using System.IO;

namespace VideoCompressorGUI.Utils
{
    public class FileSystemWatcherReferenceCounter
    {
        public FileSystemWatcher Watcher { get; set; }
        public uint ReferenceCount { get; set; }

        public FileSystemWatcherReferenceCounter(FileSystemWatcher watcher)
        {
            Watcher = watcher;
            ReferenceCount = 1;
        }

        public uint Decrease()
        {
            this.ReferenceCount--;
            return ReferenceCount;
        }

        public uint Increase()
        {
            ReferenceCount++;
            return ReferenceCount;
        }
    }
}