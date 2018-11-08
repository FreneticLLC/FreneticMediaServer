using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

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

        public override string GenerateHtmlPageFor(string category, string file, string extension, MetaFile meta)
        {
            string rawLink = Server.RawWebUrl + category + "/" + file + "." + extension;
            return GenerateBasePage(meta, rawLink, "<img class=\"media_object\" src=\"" + rawLink + "\" />");
        }

        public override byte[] Recrunch(string extension, byte[] input)
        {
            if (!Server.RebuildImages)
            {
                return input;
            }
            using (MemoryStream inputStream = new MemoryStream(input))
            {
                Image image = Image.FromStream(inputStream);
                while (image.PropertyIdList.Length != 0)
                {
                    image.RemovePropertyItem(image.PropertyIdList[0]);
                }
                using (MemoryStream outputStream = new MemoryStream())
                {
                    image.Save(outputStream, ImageFormat.Png);
                    outputStream.Flush();
                    outputStream.Seek(0, SeekOrigin.Begin);
                    return outputStream.ToArray();
                }
            }
        }
    }
}
