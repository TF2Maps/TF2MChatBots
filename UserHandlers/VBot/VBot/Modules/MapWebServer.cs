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
        MapModule mapmodule;
        readonly ObservableCollection<MapModule.Map> maplist;

        string responseString = "<HTML><BODY>Website is still initialising</BODY></HTML>";

        readonly protected string header;
        readonly protected string trailer;
        readonly protected string WebsiteFilesDirectory;
        
        Dictionary<string, string> WebsiteTables;

        //string prefix, ObservableCollection<MapModule.Map> Maplist)

        public MapWebServer(VBot bot, Dictionary<string, object> Jsconfig) : base(bot, Jsconfig)
        {
            WebsiteTables = new Dictionary<string, string>();

            try
            {

                WebsiteFilesDirectory = config["FilesDirectory"].ToString();

                Console.WriteLine("Directory set to: {0}", WebsiteFilesDirectory);
                header = System.IO.File.ReadAllText(Path.Combine(WebsiteFilesDirectory, config["HeaderFileName"].ToString()));

                trailer = System.IO.File.ReadAllText(Path.Combine(WebsiteFilesDirectory, config["TrailerFileName"].ToString()));


                StartWebServer(config["Address"].ToString());

                //StartWebServer(config["Address"].ToString());
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                Console.WriteLine("Files not found, webserver load failed");
            }
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

        

        void AddTable (string TableKey, string Data)
        {
            if (WebsiteTables.ContainsKey(TableKey))
            {
                WebsiteTables[TableKey] = Data;
            }
            else
            {
                WebsiteTables.Add(TableKey, Data);
            }
        }


        public override string getPersistentData()
        {
            throw new NotImplementedException();
        }

        public override void loadPersistentData()
        {
            throw new NotImplementedException();
        }

        void HTMLFileFromArrayListiners.HTMLFileFromArray(string[] Headernames, List<string[]> Data, string TableKey)
        {
            string Table = string.Format("<table> <caption> <h1> {0} </h1> </caption> <tbody> <tr>",TableKey);
            foreach (string value in Headernames)
            {
                Table += "<th>" + WebUtility.HtmlEncode(value) + "</th>";
            }

            Table += "</tr>";

            foreach (string[] Entry in Data)
            {
                Table += "<tr>";
                foreach (string Entryvalue in Entry)
                {
                    Table += "<td>" + WebUtility.HtmlEncode(Entryvalue) + "</td>";
                }
                Table += "</tr>";
            }
            Table += "</tbody> </table>";

            AddTable(TableKey, Table);
        }
    }
}
