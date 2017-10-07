using System.Collections.Generic;

namespace SteamBotLite
{
    public interface HTMLFileFromArrayListiners
    {
        void AddEntryWithLimit(string identifier, TableDataValue[] data, int limit);

        void AddEntryWithoutLimit(string identifier, TableDataValue[] data);

        void HTMLFileFromArray(string[] Headernames, List<string[]> Data, string TableKey);

        void MakeTableFromEntry(string TableKey, TableData TableData);

        void SetTableHeader(string TableIdentifier, TableDataValue[] Header);
    }
}