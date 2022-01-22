using System.Diagnostics;
using System.IO;

namespace VideoCompressorGUI.Utils
{
    public class UtilMethods
    {
        public static void OpenExplorerAndSelectFile(string path)
        {
            if (!File.Exists(path))
                return;

            path = path.Replace('/', '\\');
            Process.Start("explorer", $"/e, /select,\"{path}\"");
        }
    }
}