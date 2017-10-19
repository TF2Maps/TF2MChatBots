
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

    public interface IHTMLFileFromArrayPasser 
    {
        void HandleCommand(HTMLCommand command);

    }


    public abstract class HTMLCommand
    {
        public abstract void Execute(IHTMLFileFromArrayListiners listiner);
    }


    public class AddWebsiteEntry : HTMLCommand
    {
        public string Identifier;
        public TableDataValue[] Data;
        public int limit;

        public override void Execute(IHTMLFileFromArrayListiners listiner)
        {
            listiner.AddWebsiteEntry(Identifier, Data, limit);
        }
    }

    public class HTMLFileFromArray : HTMLCommand
    {
        public string[] Headernames;
        public List<string[]> Data;
        public string TableKey;

        public override void Execute(IHTMLFileFromArrayListiners listiner)
        {
            listiner.HTMLFileFromArray(Headernames, Data, TableKey);
        }
    }

    public class MakeTableFromEntry : HTMLCommand
    {
        public string TableKey;
        public TableData Data;
        public override void Execute(IHTMLFileFromArrayListiners listiner)
        {
            listiner.MakeTableFromEntry(TableKey, Data);
        }
    }

    public class SetTableHeader : HTMLCommand
    {
        public string TableIdentifier;
        public TableDataValue[] Header;

        public override void Execute(IHTMLFileFromArrayListiners listiner)
        {
            listiner.SetTableHeader(TableIdentifier, Header);
        }
    }
}
