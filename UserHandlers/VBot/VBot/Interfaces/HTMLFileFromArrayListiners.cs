using System.Collections.Generic;

namespace SteamBotLite
{
    //TODO Implement the 'command' pattern 
    public interface HTMLFileFromArrayListiners
    {

        void AddWebsiteEntryWithLimit(string identifier, TableDataValue[] data, int limit);

        void AddWebsiteEntryWithoutLimit(string identifier, TableDataValue[] data);

        void HTMLFileFromArray(string[] Headernames, List<string[]> Data, string TableKey);

        void MakeTableFromEntry(string TableKey, TableData TableData);

        void SetTableHeader(string TableIdentifier, TableDataValue[] Header);
    }
}
