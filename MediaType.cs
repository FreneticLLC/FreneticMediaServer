using System;

namespace FreneticMediaServer
{
    public abstract class MediaType
    {
        public abstract string Name { get; }

        public abstract string[] GetValidExtensions();

        public abstract string GenerateHtmlPageFor(string category, string file, string extension);
    }
}
