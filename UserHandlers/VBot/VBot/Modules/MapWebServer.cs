using System.Net;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SteamBotLite
{
    class MapWebServer : BaseModule , HTMLFileFromArrayListiners
    {
        HttpListener listener;

        string responseString = "<HTML><BODY>Website is still initialising</BODY></HTML>";

        readonly protected string header;
        readonly protected string trailer;
        readonly protected string WebsiteFilesDirectory;
        

        //string prefix, ObservableCollection<MapModule.Map> Maplist)

        public MapWebServer(VBot bot, Dictionary<string, Dictionary<string, object>> Jsconfig) : base(bot, Jsconfig)
        {
            loadPersistentData();

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
        {   }

        string GetAlltables ()
        {
            string value = "";

            foreach (KeyValuePair<string, TableData> table in DataLists)
            {
                value += table.Value.HtmlTable(table.Key);
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
            byte[] buff = System.Text.Encoding.UTF8.GetBytes("Sorry an Error Has occured");

            try
            {
                string path = (WebsiteFilesDirectory + context.Request.RawUrl);

                if (File.Exists(path))
                {
                    buff = System.Text.Encoding.UTF8.GetBytes(System.IO.File.ReadAllText(path).ToString());
                }
                else
                {
                    buff = System.Text.Encoding.UTF8.GetBytes(header + GetAlltables() + trailer);
                }
            }

            finally
            {
                response.ContentLength64 = buff.Length;
                response.Close(buff, true);
                listener.BeginGetContext(new AsyncCallback(ResponseMethod), listener);
            }

            

        }
        
        public override string getPersistentData()
        {
            return JsonConvert.SerializeObject(DataLists);
        }

        public override void loadPersistentData()
        {
            {
                try
                {
                    Console.WriteLine("Loading Website");

                    DataLists = JsonConvert.DeserializeObject<Dictionary<string, TableData>>(System.IO.File.ReadAllText(ModuleSavedDataFilePath()));
                    
                    Console.WriteLine("Loaded saved file");
                }
                catch
                {
                    DataLists = new Dictionary<string, TableData>();
                    savePersistentData();
                    Console.WriteLine("Error Loading Website Table List, creating a new one and wiping the old");
                }
            }
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

            AddTableFromEntry(TableKey, ThisTableData);
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

            AddTableFromEntry(identifier, GetTableData(identifier));
        }

        void HTMLFileFromArrayListiners.AddEntryWithoutLimit(string identifier, TableDataValue[] data)
        {
            GetTableData(identifier).TableValues.Add(data);
            AddTableFromEntry(identifier, GetTableData(identifier));
        }

        void HTMLFileFromArrayListiners.MakeTableFromEntry(string TableKey, TableData TableData)
        {
            AddTableFromEntry(TableKey, TableData);
        }

        void AddTableFromEntry(string TableKey, TableData TableData)
        {
            if (DataLists.ContainsKey(TableKey)) {
                DataLists[TableKey] = TableData;
            }
            else {
                DataLists.Add(TableKey, TableData);
            }
            savePersistentData();
        }
    }
}
