using System;
using System.Globalization;

namespace VideoCompressorGUI.Utils
{
    public static class MathHelper
    {
        /// <summary>
        /// Maps n from a range of [istart, istop] to [ostart, ostop]
        /// </summary>
        public static float Map(float value, float istart, float istop, float ostart, float ostop)
            => ostart + (ostop - ostart) * ((value - istart) / (istop - istart));
    
        public static double Map(double value, double istart, double istop, double ostart, double ostop)
            => ostart + (ostop - ostart) * ((value - istart) / (istop - istart));
    
        /// <summary>Linearly interpolating between the current and target number</summary>
        public static float Lerp(float v0, float v1, float t) {
            return (1 - t) * v0 + t * v1;
        }
    
        public static double Lerp(double v0, double v1, double t) {
            return (1 - t) * v0 + t * v1;
        }
        
        public static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString(CultureInfo.InvariantCulture) + suf[place];
        }
    }
}