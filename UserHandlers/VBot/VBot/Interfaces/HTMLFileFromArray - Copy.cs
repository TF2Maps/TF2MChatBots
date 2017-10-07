using System.Collections.Generic;

namespace SteamBotLite
{
    public interface HTMLFileFromArrayListiners
    {
        void HTMLFileFromArray(string[] Headernames, List<string[]> Data, string TableKey);

        void MakeTableFromEntry(string TableKey, TableData TableData);

        void AddEntryWithoutLimit(string identifier, TableDataValue[] data);

        void AddEntryWithLimit(string identifier, TableDataValue[] data, int limit);

        void SetTableHeader(string TableIdentifier, TableDataValue[] Header);
    }
}