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

            public const string FormButtonBasicPrefix = "<form method=\"POST\" action=\"";

            public const string FormButtonBasicMiddle01 = "\">\n<input type=\"hidden\" name=\"";

            public const string FormButtonBasicMiddle02 = "\" value=\"";

            public const string FormButtonBasicMiddle03 = "\">\n<input type=\"submit\" value=\"";

            public const string FormButtonBasicSuffix = "\">\n</form>\n";
        }

        public static string OneButtonForm(string action, string hidden_name, string hidden_value, string button_name)
        {
            return Strings.FormButtonBasicPrefix + MinimalEscape(action)
                + Strings.FormButtonBasicMiddle01 + MinimalEscape(hidden_name)
                + Strings.FormButtonBasicMiddle02 + MinimalEscape(hidden_value)
                + Strings.FormButtonBasicMiddle03 + MinimalEscape(button_name)
                + Strings.FormButtonBasicSuffix;
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
