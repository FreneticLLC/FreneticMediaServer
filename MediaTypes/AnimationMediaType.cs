using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace FreneticMediaServer.MediaTypes
{
    public class AnimationMediaType : MediaType
    {
        public override string Name => "animation";

        public static readonly string[] ValidExts = new string[]
        {
            "gif"
        };

        public override string[] GetValidExtensions()
        {
            return ValidExts;
        }

        public override string GenerateHtmlPageFor(string category, string file, string extension, MetaFile meta)
        {
            string rawLink = Server.RawWebUrl + category + "/" + file + "." + extension;
            return GenerateBasePage(meta, rawLink, rawLink, "<img class=\"media_object\" src=\"" + rawLink + "\" />");
        }

        // TODO: Recrunch?
    }
}
