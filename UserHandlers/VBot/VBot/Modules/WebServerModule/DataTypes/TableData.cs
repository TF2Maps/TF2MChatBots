using System.Collections.Generic;

namespace SteamBotLite
{
    public class TableData
    {
        public TableDataValue[] Header;
        public List<TableDataValue[]> TableValues;

        public TableData()
        {
            TableValues = new List<TableDataValue[]>();
        }

        public string HtmlTable(string Heading)
        {
            string Table = string.Format("<table> <caption> <h1> {0} </h1> </caption> <tbody> <tr>", Heading);

            if (Header != null)
            {
                foreach (TableDataValue value in Header)
                {
                    Table += "<th>" + value.GetEncodedValue() + "</th>";
                }
            }

            Table += "</tr>";

            foreach (TableDataValue[] value in TableValues)
            {
                Table += "<tr>";

                foreach (TableDataValue row in value)
                {
                    Table += "<td>" + row.GetEncodedValue() + "</td>";
                }
            }

            Table += "</tbody> </table>";

            return Table;
        }
    }
}