using System;
using Microsoft.AspNetCore.Html;

namespace FreneticMediaServer
{
    public class HtmlHelper
    {
        public class Strings
        {
            public const string HtmlHeader01 = "<!doctype HTML>\n<html>\n<head>\n<title>";

            public const string HtmlHeader02 = "</title>\n</head>\n<body>\n";

            public const string HtmlFooter = "\n</body>\n</html>\n";
        }

        public static string MinimalEscape(string input)
        {
            return input.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        public static string BasicHeaderWithTitle(string title)
        {
            return Strings.HtmlHeader01 + MinimalEscape(title) + Strings.HtmlHeader02;
        }

        public static string BasicFooter()
        {
            return Strings.HtmlFooter;
        }
    }
}
