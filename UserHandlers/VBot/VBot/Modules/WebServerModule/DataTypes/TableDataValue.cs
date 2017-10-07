using System.Net;

namespace SteamBotLite
{
    public class TableDataValue
    {
        public string VisibleValue;
        public string Link;
        public string HoverText;

        public string GetEncodedValue()
        {
            string header = "";
            string trailer = "";
            string ValueToDisplay = VisibleValue;

            if (!string.IsNullOrEmpty(HoverText))
            {
                header += "<span title = \"" + WebUtility.HtmlEncode(HoverText) + "\">";

                trailer = "</span>";
            }

            if (!string.IsNullOrEmpty(Link))
            {
                header += "<a href=\"" + WebUtility.HtmlEncode(Link) + "\">";
                trailer = "</a>" + trailer;
            }
            return header + WebUtility.HtmlEncode(ValueToDisplay) + trailer;
        }

        public TableDataValue()
        {
        }
    }
}