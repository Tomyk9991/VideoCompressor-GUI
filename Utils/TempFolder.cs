using System;
using System.IO;

namespace VideoCompressorGUI.Utils;

public static class TempFolder
{
    private static string ROOT = "";
    private static uint counter = 0;
    private static object mutex = new object();
    
    static TempFolder()
    {
        ROOT = AppDomain.CurrentDomain.BaseDirectory + "/temp";
    }
    
    public static void Clear()
    {
        // Clear folder
        DirectoryInfo di = new DirectoryInfo(ROOT);
        
        foreach (FileInfo file in di.GetFiles())
            file.Delete();
        
        foreach (DirectoryInfo directoryInfo in di.GetDirectories())
            directoryInfo.Delete(true);
    }
    
    public static string GenerateNewName(string extension)
    {
        // Creates if needed
        Directory.CreateDirectory(ROOT);

        lock (mutex)
        {
            counter++;
        }
        
        string base64Guid = "_" + counter; //Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        return ROOT + "/" + base64Guid + extension;
    }
}