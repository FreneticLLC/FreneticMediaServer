using System;

namespace FreneticMediaServer
{
    public abstract class MediaType
    {
        public Startup Server;

        public abstract string Name { get; }

        public abstract string[] GetValidExtensions();

        public abstract string GenerateHtmlPageFor(string category, string file, string extension);

        public virtual byte[] Recrunch(string extension, byte[] input)
        {
            return input;
        }
    }
}
