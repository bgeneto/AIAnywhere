using OpenAI.Images;

namespace AIAnywhere.Models
{
    /// <summary>
    /// Custom image sizes for portrait and landscape orientations not defined in OpenAI.GeneratedImageSize.
    /// </summary>
    public static class CustomImageSizes
    {
        // 1:1 Square sizes
        public static readonly GeneratedImageSize W512xH512 = GeneratedImageSize.W512xH512;
        public static readonly GeneratedImageSize W768xH768 = new GeneratedImageSize(768, 768);
        public static readonly GeneratedImageSize W1024xH1024 = GeneratedImageSize.W1024xH1024;

        // 2:3 Portrait sizes
        public static readonly GeneratedImageSize W512xH768 = new GeneratedImageSize(512, 768);
        public static readonly GeneratedImageSize W768xH1152 = new GeneratedImageSize(768, 1152);
        public static readonly GeneratedImageSize W832xH1248 = new GeneratedImageSize(832, 1248);
        public static readonly GeneratedImageSize W896xH1344 = new GeneratedImageSize(896, 1344);
        public static readonly GeneratedImageSize W1024xH1536 = new GeneratedImageSize(1024, 1536);

        // 3:2 Landscape sizes
        public static readonly GeneratedImageSize W768xH512 = new GeneratedImageSize(768, 512);
        public static readonly GeneratedImageSize W1152xH768 = new GeneratedImageSize(1152, 768);
        public static readonly GeneratedImageSize W1248xH832 = new GeneratedImageSize(1248, 832);
        public static readonly GeneratedImageSize W1344xH896 = new GeneratedImageSize(1344, 896);
        public static readonly GeneratedImageSize W1536xH1024 = new GeneratedImageSize(1536, 1024);

        // 3:4 Portrait sizes
        public static readonly GeneratedImageSize W768xH1024 = new GeneratedImageSize(768, 1024);
        public static readonly GeneratedImageSize W936xH1248 = new GeneratedImageSize(936, 1248);

        // 4:3 Landscape sizes
        public static readonly GeneratedImageSize W1024xH768 = new GeneratedImageSize(1024, 768);
        public static readonly GeneratedImageSize W1248xH936 = new GeneratedImageSize(1248, 936);
    }
}
