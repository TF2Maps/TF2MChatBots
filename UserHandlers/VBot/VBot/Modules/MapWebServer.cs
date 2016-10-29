using System.Net;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System;
using System.IO;

namespace SteamBotLite
{
    class MapWebServer
    {
        HttpListener listener;
        MapModule mapmodule;
        string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";

        public MapWebServer (string prefix , MapModule VBotMapmodule)
        {
            mapmodule = VBotMapmodule;
            mapmodule.mapList.CollectionChanged += MapListUpdate;
            StartWebServer(prefix);
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

            MapListUpdate(this, null);
        }
        public void CloseWebServer ()
        {
           // listener.Stop();
          //  listener.Close();
        }

        void ResponseMethod(IAsyncResult result )
        {
            HttpListener listener = (HttpListener)result.AsyncState;
            
           
            HttpListenerContext context = listener.EndGetContext(result);
            
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

            HttpListenerResponse response = context.Response;
            byte[] buff = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buff.Length;
            
            response.Close(buff, true);
            listener.BeginGetContext(new AsyncCallback(ResponseMethod), listener);
            
        }

        public void MapListUpdate(object sender, NotifyCollectionChangedEventArgs args)
        {
            //PRONE TO INJECTION FIX
            string Header = "<html><body> <table> <tr> <th> MapName</th> <th> Url </th>";
            string MapDataCache = "";
            string Form = "<form action=\"demo_form.asp\"method=\"get\">Map Name: <input type =\"text\" name=\"fname\"><br> Map Url: <input type =\"text\" name=\"lname\"><br><button type =\"submit\">Submit</button><button type =\"submit\" formmethod=\"POST\" formaction=\"index\">Submit using POST</button></ form > ";

            string Trailer = "</table>" /*+ Form*/  + "</body> </html>";

            foreach (MapModule.Map map in mapmodule.mapList)
            {
                MapDataCache += "<tr> <td>" + WebUtility.HtmlEncode(map.Filename) + "</td>" + "<td>" + WebUtility.HtmlEncode(map.DownloadURL) + "</td> </tr>";
            }
            responseString = Header + MapDataCache + Trailer;

        }
    }
}
