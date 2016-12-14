using System.Net;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System;
using System.IO;
using System.Collections.Generic;

namespace SteamBotLite
{
    class MapWebServer : BaseModule
    {
        HttpListener listener;
        MapModule mapmodule;
        readonly ObservableCollection<MapModule.Map> maplist;

        string responseString = "<HTML><BODY>Website is still initialising</BODY></HTML>";

        readonly protected string header;
        readonly protected string trailer;
        readonly protected string WebsiteFilesDirectory;
        string MapDataCache;

        //string prefix, ObservableCollection<MapModule.Map> Maplist)

        public MapWebServer (VBot bot, Dictionary<string, object> Jsconfig) : base (bot, Jsconfig)
        {

            //maplist = new ObservableCollection<MapModule.Map>(Maplist);        
            try
            {
                //string data = config["HeaderFilePath"].ToString();
              //  Console.WriteLine(data);

                WebsiteFilesDirectory = config["FilesDirectory"].ToString();

                Console.WriteLine("Directory set to: {0}", WebsiteFilesDirectory);
                header = System.IO.File.ReadAllText(Path.Combine(WebsiteFilesDirectory , config["HeaderFileName"].ToString()));
                
                trailer = System.IO.File.ReadAllText(Path.Combine (WebsiteFilesDirectory , config["TrailerFileName"].ToString()));

                
                StartWebServer(config["Address"].ToString());
                
                //StartWebServer(config["Address"].ToString());
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                Console.WriteLine("Files not found, webserver load failed");
            }
        }

        //When this class is turned off, we close the server properly
        ~MapWebServer ()
        {
            CloseWebServer();
        }
        
        public void StartWebServer (string prefix)
        {
            Console.WriteLine("Website Loding");
            listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            listener.Start();
            
            listener.BeginGetContext(new AsyncCallback(ResponseMethod), listener);
            
            Console.WriteLine("Website Loaded");

            //MapListUpdate(this, null);
        }
        public void CloseWebServer ()
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
                buff = System.Text.Encoding.UTF8.GetBytes(header + MapDataCache + trailer);
            }          
            

            /*
            Console.WriteLine(context.Request.HttpMethod);
            if (context.Request.HttpMethod.Equals("POST"))
            {
                string text;
                using (var reader = new StreamReader(context.Request.InputStream,
                                                     context.Request.ContentEncoding))
                {
                    text = reader.ReadToEnd();
                }

            }
            */

           
            
            response.ContentLength64 = buff.Length;
            
            response.Close(buff, true);
            listener.BeginGetContext(new AsyncCallback(ResponseMethod), listener);
            
        }

        public void MapListUpdate(ObservableCollection<MapModule.Map> maplist)
        {
            MapDataCache = "";

            foreach (MapModule.Map map in maplist)
            {
                MapDataCache += "<tr>";
                MapDataCache += "<td>" + WebUtility.HtmlEncode(map.Filename) + "</td>";
                MapDataCache += "<td> <a href=\"" + WebUtility.HtmlEncode(map.DownloadURL) + "\">" + WebUtility.HtmlEncode(map.DownloadURL) + "</a></td>";
                MapDataCache += "<td>" + WebUtility.HtmlEncode(map.Notes) + "</td>";
                MapDataCache += "<td> <span title = \"" + WebUtility.HtmlEncode(map.Submitter.ToString()) + "\">" +  WebUtility.HtmlEncode(map.SubmitterName) + "</span> </td>";
                MapDataCache += "</tr>";
            }
            
            //string Form = "<form action=\"demo_form.asp\"method=\"get\">filename: <input type =\"text\" name=\"fname\"><br> Map Url: <input type =\"text\" name=\"lname\"><br><button type =\"submit\">Submit</button><button type =\"submit\" formmethod=\"POST\" formaction=\"index\">Submit using POST</button></ form > ";

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
