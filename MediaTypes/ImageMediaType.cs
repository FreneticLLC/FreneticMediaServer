using System;

namespace FreneticMediaServer.MediaTypes
{
    public class ImageMediaType : MediaType
    {
        public override string Name => "image";

        public static readonly string[] ValidExts = new string[]
        {
            "jpg", "jpeg", "png"
        };

        public override string[] GetValidExtensions()
        {
            return ValidExts;
        }

        public override string GenerateHtmlPageFor(string category, string file, string extension)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
