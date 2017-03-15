using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SteamBotLite
{
    public class TableDataValue
    {
        public string VisibleValue;
        public string Link;
        public string HoverText;

        public string GetEncodedValue ()
        {
            string header = "";
            string trailer = "";
            string ValueToDisplay = VisibleValue;
            
            if ( !string.IsNullOrEmpty(HoverText))
            {
                header += "<span title = \"";
                trailer = trailer + "\">" + WebUtility.HtmlEncode(VisibleValue) + "</span>" ;
                ValueToDisplay = HoverText;
            }
            if ( !string.IsNullOrEmpty(Link))
            {
                header += "<a href=\"";
                trailer = trailer + "\">" + WebUtility.HtmlEncode(VisibleValue ) + "</a>";
            }
            return header + WebUtility.HtmlEncode(ValueToDisplay) + trailer;
        }

        public TableDataValue() { }
    }
}
