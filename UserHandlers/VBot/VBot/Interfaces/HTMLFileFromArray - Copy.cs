using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamBotLite
{
    public interface HTMLFileFromArrayListiners
    {
        void HTMLFileFromArray(string[] Headernames, List<string[]> Data, string TableKey);

        void AddHTMLTable(string TableKey, string Tabledata);

        void MakeTableFromEntry(string TableKey, TableData TableData);

        void AddEntryWithoutLimit(string identifier, TableDataValue[] data);

        void AddEntryWithLimit(string identifier, TableDataValue[] data, int limit);

        void SetTableHeader(string TableIdentifier, TableDataValue[] Header);
    }
}
