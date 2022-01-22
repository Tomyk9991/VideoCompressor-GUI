using System;
using System.IO;
using System.Text;

namespace VideoCompressorGUI.Utils
{
    public static class TempFolder
    {
        private static string ROOT = "";
    
        static TempFolder()
        {
            ROOT = AppDomain.CurrentDomain.BaseDirectory + "/temp";
            Directory.CreateDirectory(ROOT);
        }
    
        public static void ClearOnTimeExpired()
        {
            // Clear folder
            DirectoryInfo di = new DirectoryInfo(ROOT);

            bool cleanup = false;
            foreach (FileInfo file in di.GetFiles())
            {
                if (DateTime.Now - file.CreationTime > TimeSpan.FromDays(7))
                {
                    cleanup = true;
                    Console.WriteLine("Clean up temp folder");
                    break;
                }
            }

            if (cleanup)
            {
                // Clear folder
                foreach (FileInfo file in di.GetFiles())
                    file.Delete();
        
                foreach (DirectoryInfo directoryInfo in di.GetDirectories())
                    directoryInfo.Delete(true);
            }
        }

        public static string GenerateThumbnailName(in DateTime creationTime, string filePath, string extension)
        {
            var creationString = creationTime.ToShortDateString();
            var fileName = Path.GetFileNameWithoutExtension(filePath);
        
            // Creates if needed
            Directory.CreateDirectory(ROOT);

            StringBuilder builder = new StringBuilder(ROOT);
            builder.Append("/").Append(fileName).Append('-').Append(creationString).Append(extension);
        
            return builder.ToString();
        }
    }
}