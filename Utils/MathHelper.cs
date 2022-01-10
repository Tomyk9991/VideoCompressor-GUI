namespace VideoCompressorGUI.Utils;

public static class MathHelper
{
    /// <summary>
    /// Maps n from a range of [istart, istop] to [ostart, ostop]
    /// </summary>
    public static float Map(float value, float istart, float istop, float ostart, float ostop)
        => ostart + (ostop - ostart) * ((value - istart) / (istop - istart));
    
    public static double Map(double value, double istart, double istop, double ostart, double ostop)
        => ostart + (ostop - ostart) * ((value - istart) / (istop - istart));
}