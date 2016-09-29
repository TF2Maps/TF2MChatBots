using System.Net;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System;

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

        public MapWebServer(string prefix)
        {
            StartWebServer(prefix);
        }

        public MapWebServer()
        {
        }

        //When this class is turned off, we close the server properly
        ~MapWebServer ()
        {
            CloseWebServer();
        }
        public void start()
        {
            StartWebServer();
        }
        
        public void StartWebServer (string prefix = ("http://localhost:2048/index/"))
        {
            Console.WriteLine("Website Loding");
            listener = new HttpListener();
            listener.Prefixes.Add(prefix);

            listener.Start();
            listener.BeginGetContext(new AsyncCallback(ResponseMethod), listener);

            /*

            // Note: The GetContext method blocks while waiting for a request. 
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;
            // Obtain a response object.
            HttpListenerResponse response = context.Response;

            string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            */
            Console.WriteLine("Website Loaded");

            //MapListUpdate(this, null);
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
            HttpListenerResponse response = context.Response;
            byte[] buff = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buff.Length;
            
            response.Close(buff, true);
            listener.BeginGetContext(new AsyncCallback(ResponseMethod), listener);
            
        }

        public void MapListUpdate(object sender, NotifyCollectionChangedEventArgs args)
        {
            return;
            string Header = "< HTML >< BODY > <table> <tr> <th> MapName</th> <th> Url </th>";
            string MapDataCache = "";
            string Trailer = "</table> </body> </html>";
            foreach (MapModule.Map map in mapmodule.mapList)
            {
                MapDataCache += "<tr> <td>" + map.Filename + "</td>" + "<td>" + map.DownloadURL + "</td> </tr>";
            }
            responseString = Header + MapDataCache + Trailer;

        }
    }
}
