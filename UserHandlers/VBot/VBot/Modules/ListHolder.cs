using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SteamBotLite
{
    public class ListHolder : BaseModule
    {
        ModuleHandler HtmlParserThing;
        public ListHolder(ModuleHandler handler,  Dictionary<string, Dictionary<string, object>> Jsconfig) : base(handler, Jsconfig)
        {
            HtmlParserThing = handler;
            DataLists = new Dictionary<string, TableData>();
            commands.Add(new FeatureRequest(handler, this));
        }

        private class FeatureRequest : BaseCommand
        {
            ListHolder module;

            public FeatureRequest(ModuleHandler bot, ListHolder module) : base(bot, "!Request")
            {

                this.module = module;

                //Setup Table 

                TableDataValue UsernameHeader = new TableDataValue();
                UsernameHeader.VisibleValue ="Username";
                TableDataValue Message = new TableDataValue();
                Message.VisibleValue = "Message";

                module.SetTableHeader("Requests", new TableDataValue[] { UsernameHeader, Message });
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                if ( !param.Equals("!Request"))
                {
                    TableDataValue Username = new TableDataValue();
                    Username.VisibleValue = Msg.Sender.identifier.ToString();
                    TableDataValue Message = new TableDataValue();
                    Message.VisibleValue = param;

                    TableDataValue[] Data = new TableDataValue[] { Username, Message };
                    module.AddEntryWithoutLimit("Requests", Data);
                    module.AddEntryWithLimit("RequestsWithLimit", Data, 3);

                
                    return "Added Entry";
                }
                else
                {
                    return "Actually put in something";
                }
            }
        }

        HTMLFileFromArrayListiners handler;

        public class TableDataValue
        {
            public string VisibleValue;
            string Link;
            string HoverText;
        }

        public class TableData
        {
            public TableDataValue[] Header;
            public List<TableDataValue[]> TableValues;

            public TableData()
            {
                TableValues = new List<TableDataValue[]>();
            }
        }

        Dictionary<string, TableData> DataLists;

        public void SetTableHeader (string TableIdentifier, TableDataValue[] Header  )
        {
            GetTableData(TableIdentifier).Header = Header;
        }

        //Does this pass by value or by reference? 

        public TableData GetTableData (string identifier)
        {
            if (DataLists.ContainsKey(identifier))
            {
                return DataLists[identifier];
            }
            else
            {
                DataLists.Add(identifier, new TableData());
                return DataLists[identifier];
            }
        }

        public void AddEntryWithLimit (string identifier, TableDataValue[] data , int limit)
        {
            GetTableData(identifier).TableValues.Add(data);

            int ExcessToRemove = GetTableData(identifier).TableValues.Count - limit;


            int entriestoremove = limit; 

            if (GetTableData(identifier).TableValues.Count > entriestoremove) {
                GetTableData(identifier).TableValues.RemoveRange(0, ExcessToRemove);
            }

            MakeTableFromEntry(identifier, GetTableData(identifier));
        }

        void AddEntryWithoutLimit(string identifier, TableDataValue[] data)
        {
            GetTableData(identifier).TableValues.Add(data);
            MakeTableFromEntry(identifier, GetTableData(identifier));
        }

        void MakeTableFromEntry (string TableKey , TableData TableData )
        {
            string Table = string.Format("<table> <caption> <h1> {0} </h1> </caption> <tbody> <tr>", TableKey);

            if (TableData.Header != null)
            {
                foreach (TableDataValue value in TableData.Header)
                {
                    Table += "<th>" + WebUtility.HtmlEncode(value.VisibleValue) + "</th>";
                }
            }

            Table += "</tr>";

            foreach (TableDataValue[] value in TableData.TableValues)
            {
                Table += "<tr>";

                foreach (TableDataValue row in value)
                {
                    Table += "<td>" + WebUtility.HtmlEncode(row.VisibleValue) + "</td>";
                }
            }

            Table += "</tbody> </table>";

            HtmlParserThing.AddHTMLTable(TableKey, Table);

        }

        public override void OnAllModulesLoaded()
        {
           
        }

        public override string getPersistentData()
        {
            throw new NotImplementedException();
        }

        public override void loadPersistentData()
        {
            throw new NotImplementedException();
        }
    }
}
