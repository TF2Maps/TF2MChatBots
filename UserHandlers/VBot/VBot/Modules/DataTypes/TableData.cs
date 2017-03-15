using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }

    

}
