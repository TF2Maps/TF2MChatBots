using System;
using System.Net;
using System.IO;
using System.Net.Cache;

namespace SteamBotLite
{
    public class SearchClassEntry
    {
        public string URLPrefix { get; set; }
        public string URLSuffix { get; set; }
        public string Command { get; set; }
        public string SpiderPrefix { get; set; }
        public string SpiderSuffix { get; set; }
        public bool IsCustomUrl { get; set; }
    }
    static class SearchClass
    {
        public static string Search(SearchClassEntry SearchEntry, string SearchURL)
        {

            // Set a default policy level for the "http:" and "https" schemes.
            HttpRequestCachePolicy policy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            HttpWebRequest.DefaultCachePolicy = policy;

            Console.WriteLine("Searching...");

            WebRequest wrGETURL;
            

            if (SearchEntry.IsCustomUrl)
                wrGETURL = WebRequest.Create(SearchEntry.URLPrefix + SearchURL + SearchEntry.URLSuffix);
            else
                wrGETURL = WebRequest.Create(SearchEntry.URLPrefix + SearchEntry.URLSuffix);

            wrGETURL.CachePolicy = policy;

            Stream objStream;

            objStream = wrGETURL.GetResponse().GetResponseStream();

            StreamReader objReader = new StreamReader(objStream);

            

            string HttpData = objReader.ReadToEnd();

            objStream.Close();
            objStream.Dispose();

            objReader.Close();
            objReader.Dispose();

            wrGETURL.GetResponse().Close();
            wrGETURL.GetResponse().Dispose();

            string[] SpideredData = HttpData.Split(new string[] { SearchEntry.SpiderPrefix }, StringSplitOptions.RemoveEmptyEntries);
            if (SpideredData.Length > 1)
            {
                SpideredData = SpideredData[1].Split(new string[] { SearchEntry.SpiderSuffix }, StringSplitOptions.RemoveEmptyEntries);
                return SpideredData[0];
            }
            else
            {
                return "Invalid Search";
            }


        }
        
    }
}
