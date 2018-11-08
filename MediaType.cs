using System;

namespace FreneticMediaServer
{
    public abstract class MediaType
    {
        public Startup Server;

        public abstract string Name { get; }

        public abstract string[] GetValidExtensions();

        public abstract string GenerateHtmlPageFor(string category, string file, string extension, MetaFile meta);

        public string HtmlSafe(string text)
        {
            return text.Replace("&", "&amp;").Replace("\r", "").Replace("<", "&lt;").Replace(">", "&gt;").Replace("$", "&#36;").Replace("\"", "&quot;");
        }

        public string TextifyBasic(DateTimeOffset date)
        {
            return date.Year + "/" + date.Month.ToString().PadLeft(2, '0') + "/" + date.Day.ToString().PadLeft(2, '0')
                + " " + date.Hour.ToString().PadLeft(2, '0') + ":" + date.Minute.ToString().PadLeft(2, '0') + ":" + date.Second.ToString().PadLeft(2, '0');
        }

        public string Textify(DateTimeOffset date)
        {
            string offset;
            if (date.Offset.Hours < 0)
            {
                offset = "-" + (-date.Offset.Hours).ToString().PadLeft(2, '0');
            }
            else
            {
                offset = "+" + date.Offset.Hours.ToString().PadLeft(2, '0');
            }
            return TextifyBasic(date) + "<span class=\"minor_date_info\">" + " UTC" + offset + "</span>";
        }

        public string GenerateBasePage(MetaFile meta, string rawLink, string imageLink, string embedText)
        {
            string headers;
            if (imageLink == null)
            {
                headers = "";
            }
            else
            {
                headers = 
                "<link rel=\"image_src\" href=\"" + imageLink + "\">"
                + "<meta name=\"image\" content=\"" + imageLink + "\">"
                + "<meta name=\"og:image\" content=\"" + imageLink + "\">"
                + "<meta name=\"twitter:image\" content=\"" + imageLink + "\">"
                + "<meta property=\"image\" content=\"" + imageLink + "\">"
                + "<meta property=\"og:image\" content=\"" + imageLink + "\">"
                + "<meta property=\"twitter:image\" content=\"" + imageLink + "\">";
            }
            string page = Startup.Page_Ref_FileView;
            page = page.Replace("$NAME$", HtmlSafe(meta.OriginalName));
            page = page.Replace("$TYPE$", HtmlSafe(Name));
            page = page.Replace("$DESCRIPTION$", HtmlSafe(meta.Description).Replace("\n", "\n<br>"));
            page = page.Replace("$DATE$", Textify(meta.Time));
            page = page.Replace("$DATE_SHORT$", TextifyBasic(meta.Time));
            page = page.Replace("$UPLOADER$", HtmlSafe(meta.Uploader));
            page = page.Replace("$CONTACT_EMAIL$", HtmlSafe(Server.ContactEmail));
            page = page.Replace("$FILE_EMBED$", embedText);
            page = page.Replace("$RAW_LINK$", rawLink);
            page = page.Replace("$IMAGE_HEADERS$", headers);
            return page;
        }

        public virtual byte[] Recrunch(string extension, byte[] input)
        {
            return input;
        }
    }
}
