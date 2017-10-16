using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace SteamBotLite
{
    internal partial class WebServerHostingModule : BaseModule, IHTMLFileFromArrayListiners
    {
        readonly protected string header;
        readonly protected string trailer;
        readonly protected string WebsiteFilesDirectory;
        private Dictionary<string, TableData> DataLists;
        private HttpListener listener;

        private string responseString = "<HTML><BODY>Website is still initialising</BODY></HTML>";
        //string prefix, ObservableCollection<MapModule.Map> Maplist)

        public WebServerHostingModule(VBot bot, Dictionary<string, Dictionary<string, object>> Jsconfig) : base(bot, Jsconfig)
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

            adminCommands.Add(new RebootWebModule(bot, this));
            adminCommands.Add(new RemoveTable(bot, this));
        }

        //When this class is turned off, we close the server properly
        ~WebServerHostingModule()
        {
            CloseWebServer();
        }

        void IHTMLFileFromArrayListiners.AddWebsiteEntry(string identifier, TableDataValue[] data, int limit)
        {
            GetTableData(identifier).TableValues.Add(data);

            int CurrentTableCount = GetTableData(identifier).TableValues.Count;
            int ExcessToRemove = CurrentTableCount - limit;

            int entriestoremove = limit;

            if ( (limit > 0) & (CurrentTableCount > limit))
            {
                GetTableData(identifier).TableValues.RemoveRange(0, ExcessToRemove);
            }

            AddTableFromEntry(identifier, GetTableData(identifier));
        }


        public void CloseWebServer()
        {
            Console.WriteLine("Closing Web Server");
            listener.Stop();
            listener.Close();
        }

        public override string getPersistentData()
        {
            return JsonConvert.SerializeObject(DataLists);
        }

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

        //For legacy reasons
        void IHTMLFileFromArrayListiners.HTMLFileFromArray(string[] Headernames, List<string[]> Data, string TableKey)
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

        //Does this pass by value or by reference?
        void IHTMLFileFromArrayListiners.MakeTableFromEntry(string TableKey, TableData TableData)
        {
            AddTableFromEntry(TableKey, TableData);
        }

        public override void OnAllModulesLoaded()
        { }

        public void StartWebServer(string prefix)
        {
            Console.WriteLine("Website Loding");
            listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            listener.Start();

            listener.BeginGetContext(new AsyncCallback(ResponseMethod), listener);

            Console.WriteLine("Website Loaded");

        }

        private void AddTableFromEntry(string TableKey, TableData TableData)
        {
            if (DataLists.ContainsKey(TableKey))
            {
                DataLists[TableKey] = TableData;
            }
            else
            {
                DataLists.Add(TableKey, TableData);
            }
            savePersistentData();
        }

        private string GetAlltables()
        {
            string value = "";

            foreach (KeyValuePair<string, TableData> table in DataLists)
            {
                value += table.Value.HtmlTable(table.Key);
            }
            return value;
        }

        private void RemoveTableFromEntry(string TableKey)
        {
            if (DataLists.ContainsKey(TableKey))
            {
                DataLists.Remove(TableKey);
            }
            else
            {
            }
            savePersistentData();
        }

        private void ResponseMethod(IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState;

            HttpListenerContext context = listener.EndGetContext(result);
            HttpListenerResponse response = context.Response;
            byte[] buff = System.Text.Encoding.UTF8.GetBytes("Sorry an Error Has occured");

            try
            {
                string WebsitePath = (WebsiteFilesDirectory + context.Request.RawUrl);
                string InternalFilesPath = userhandler.GetType().Name + context.Request.RawUrl;

                string[] PathsToLookIn = { WebsitePath, InternalFilesPath };

                buff = System.Text.Encoding.UTF8.GetBytes(header + GetAlltables() + trailer);

                foreach (string path in PathsToLookIn)
                {
                    if (File.Exists(path))
                    {
                        buff = System.Text.Encoding.UTF8.GetBytes(System.IO.File.ReadAllText(path).ToString());
                    }
                }
            }
            finally
            {
                response.ContentLength64 = buff.Length;
                response.Close(buff, true);
                listener.BeginGetContext(new AsyncCallback(ResponseMethod), listener);
            }
        }

        void IHTMLFileFromArrayListiners.SetTableHeader(string TableIdentifier, TableDataValue[] Header)
        {
            GetTableData(TableIdentifier).Header = Header;
        }

        private void SetTableHeader(string tableKey, TableDataValue[] header)
        {
            SetTableHeader(tableKey, header);
        }
    }
}