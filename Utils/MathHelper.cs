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
    }
}