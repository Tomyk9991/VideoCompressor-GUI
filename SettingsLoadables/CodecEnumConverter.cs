using FFmpeg.NET.Enums;

namespace VideoCompressorGUI.SettingsLoadables
{
    [System.Serializable]
    public enum CodecDTO
    {
        StandardCodec,
        h264_nvenc
    }
    
    public class CodecEnumConverter
    {
        public static CodecDTO VideoCodecToCodecDTO(VideoCodec codec)
        {
            return codec switch
            {
                VideoCodec.h264_nvenc => CodecDTO.h264_nvenc,
                _ => CodecDTO.StandardCodec
            };
        }

        public static VideoCodec CodecDTOToVideoCodec(CodecDTO dto)
        {
            return dto switch
            {
                CodecDTO.StandardCodec => VideoCodec.Default,
                CodecDTO.h264_nvenc => VideoCodec.h264_nvenc,
                _ => VideoCodec.Default
            };
        }
    }
    
    
}