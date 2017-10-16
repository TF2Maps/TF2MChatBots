
using System.Collections.Generic;

namespace SteamBotLite
{
    //TODO Implement the 'command' pattern 
    public interface IHTMLFileFromArrayListiners
    {
        void AddWebsiteEntry(string identifier, TableDataValue[] data, int limit);

        void HTMLFileFromArray(string[] Headernames, List<string[]> Data, string TableKey);

        void MakeTableFromEntry(string TableKey, TableData TableData);

        void SetTableHeader(string TableIdentifier, TableDataValue[] Header);
    }


    public abstract class HTMLCommand
    {
        string Identifier;
        TableDataValue[] Data;
        int limit;
        string TableKey;
        public abstract void Execute();
    }
    
}
