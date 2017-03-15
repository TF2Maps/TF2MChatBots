using System.Net;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System;
using System.IO;
using System.Collections.Generic;

namespace SteamBotLite
{
    class MapWebServer : BaseModule , HTMLFileFromArrayListiners
    {
        HttpListener listener;

        string responseString = "<HTML><BODY>Website is still initialising</BODY></HTML>";

        readonly protected string header;
        readonly protected string trailer;
        readonly protected string WebsiteFilesDirectory;
        
        Dictionary<string, string> WebsiteTables;

        //string prefix, ObservableCollection<MapModule.Map> Maplist)

        public MapWebServer(VBot bot, Dictionary<string, Dictionary<string, object>> Jsconfig) : base(bot, Jsconfig)
        {
            WebsiteTables = new Dictionary<string, string>();
            DataLists = new Dictionary<string, TableData>();

            try
            {
                WebsiteFilesDirectory = config["FilesDirectory"].ToString();
                header = System.IO.File.ReadAllText(Path.Combine(WebsiteFilesDirectory, config["HeaderFileName"].ToString()));
                trailer = System.IO.File.ReadAllText(Path.Combine(WebsiteFilesDirectory, config["TrailerFileName"].ToString()));

                StartWebServer(config["Address"].ToString());

                bot.HTMLParsers.Add(this);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                Console.WriteLine("Files not found, webserver load failed");
            }

            adminCommands.Add(new RebootModule(bot, this));
        }

        private class RebootModule : BaseCommand
        {
            // Command to query if a server is active
            MapWebServer module;
            ModuleHandler ModuleHandler;
            string address;

            public RebootModule(ModuleHandler bot, MapWebServer module) : base(bot, "!WebsiteReboot")
            {
                this.module = module;
                this.address = (module.config["Address"].ToString());
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                module.CloseWebServer();
                module.StartWebServer(address);
                return "Rebooting Serer";
            }

        }


        public override void OnAllModulesLoaded()
        {

        }

        string GetAlltables ()
        {
            string value = "";
            foreach (KeyValuePair<string,string> table in WebsiteTables)
            {
                value += table.Value;
            }
            return value;
        }

        //When this class is turned off, we close the server properly
        ~MapWebServer()
        {
            CloseWebServer();
        }

        public void StartWebServer(string prefix)
        {
            Console.WriteLine("Website Loding");
            listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            listener.Start();

            listener.BeginGetContext(new AsyncCallback(ResponseMethod), listener);

            Console.WriteLine("Website Loaded");

            //MapListUpdate(this, null);
        }
        public void CloseWebServer()
        {
            Console.WriteLine("Closing Web Server");
            listener.Stop();
            listener.Close();
        }

        void ResponseMethod(IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState;


            HttpListenerContext context = listener.EndGetContext(result);
            HttpListenerResponse response = context.Response;
            string path = (WebsiteFilesDirectory + context.Request.RawUrl);

            byte[] buff;

            if (File.Exists(path))
            {
                buff = System.Text.Encoding.UTF8.GetBytes(System.IO.File.ReadAllText(path).ToString());
            }
            else
            {
                buff = System.Text.Encoding.UTF8.GetBytes(header + GetAlltables() + trailer);
            }

            response.ContentLength64 = buff.Length;

            response.Close(buff, true);
            listener.BeginGetContext(new AsyncCallback(ResponseMethod), listener);

        }
        //TODO Maybe we could keep this? 

        /*
        public void MapListUpdate(ObservableCollection<MapModule.Map> maplist)
        {
            MapDataCache = "";

            foreach (MapModule.Map map in maplist)
            {
                MapDataCache += "<tr>";
                MapDataCache += "<td>" + WebUtility.HtmlEncode(map.Filename) + "</td>";
                MapDataCache += "<td> <a href=\"" + WebUtility.HtmlEncode(map.DownloadURL) + "\">" + WebUtility.HtmlEncode(map.DownloadURL) + "</a></td>";
                MapDataCache += "<td>" + WebUtility.HtmlEncode(map.Notes) + "</td>";
                MapDataCache += "<td> <span title = \"" + WebUtility.HtmlEncode(map.Submitter.ToString()) + "\">" + WebUtility.HtmlEncode(map.SubmitterName) + "</span> </td>";
                MapDataCache += "</tr>";
            }

            //string Form = "<form action=\"demo_form.asp\"method=\"get\">filename: <input type =\"text\" name=\"fname\"><br> Map Url: <input type =\"text\" name=\"lname\"><br><button type =\"submit\">Submit</button><button type =\"submit\" formmethod=\"POST\" formaction=\"index\">Submit using POST</button></ form > ";

        }
        */
        /*
        void HTMLFileFromArrayListiners.AddHTMLTable(string TableKey, string Tabledata)
        {
            throw new NotImplementedException();
        }
        */

        void AddHTMLTable(string TableKey, string Tabledata)
        {
            if (WebsiteTables.ContainsKey(TableKey))
            {
                WebsiteTables[TableKey] = Tabledata;
            }
            else
            {
                WebsiteTables.Add(TableKey, Tabledata);
            }
        }

        void HTMLFileFromArrayListiners.AddHTMLTable(string TableKey, string Tabledata)
        {
            AddHTMLTable(TableKey, Tabledata);
        }

        public override string getPersistentData()
        {
            throw new NotImplementedException();
        }

        public override void loadPersistentData()
        {
            throw new NotImplementedException();
        }


        //For legacy reasons
        void HTMLFileFromArrayListiners.HTMLFileFromArray(string[] Headernames, List<string[]> Data, string TableKey)
        {
            TableData ThisTableData = new TableData();

            TableDataValue[] Header = new TableDataValue[Headernames.Length];
            for (int i = 0; i < Headernames.Length; i++)
            {
                Header[i] = new TableDataValue();
                Header[i].VisibleValue = Headernames[i];
            }

            ThisTableData.Header = Header;
            

            foreach (string[] Row in Data)
            {
                TableDataValue[] DataEntries = new TableDataValue[Row.Length];

                for (int i = 0; i < Row.Length; i++)
                {
                    DataEntries[i] = new TableDataValue();
                    DataEntries[i].VisibleValue = Row[i];
                }

                ThisTableData.TableValues.Add(DataEntries);
            }

            MakeTableFromEntry(TableKey, ThisTableData);
        }

        

        private void SetTableHeader(string tableKey, TableDataValue[] header)
        {
            SetTableHeader(tableKey, header);
        }

        Dictionary<string, TableData> DataLists;

        void HTMLFileFromArrayListiners.SetTableHeader(string TableIdentifier, TableDataValue[] Header)
        {
            GetTableData(TableIdentifier).Header = Header;
        }

        //Does this pass by value or by reference? 

        public TableData GetTableData(string identifier)
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

        void HTMLFileFromArrayListiners.AddEntryWithLimit(string identifier, TableDataValue[] data, int limit)
        {
            GetTableData(identifier).TableValues.Add(data);

            int ExcessToRemove = GetTableData(identifier).TableValues.Count - limit;


            int entriestoremove = limit;

            if (GetTableData(identifier).TableValues.Count > entriestoremove)
            {
                GetTableData(identifier).TableValues.RemoveRange(0, ExcessToRemove);
            }

            MakeTableFromEntry(identifier, GetTableData(identifier));
        }

        void HTMLFileFromArrayListiners.AddEntryWithoutLimit(string identifier, TableDataValue[] data)
        {
            GetTableData(identifier).TableValues.Add(data);
            MakeTableFromEntry(identifier, GetTableData(identifier));
        }

        void HTMLFileFromArrayListiners.MakeTableFromEntry(string TableKey, TableData TableData)
        {
            MakeTableFromEntry(TableKey, TableData);
        }

        void MakeTableFromEntry(string TableKey, TableData TableData)
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
            AddHTMLTable(TableKey, Table);
        }

       
    }
}
