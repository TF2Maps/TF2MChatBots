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
        public static string Search(SearchClassEntry SearchEntry, string SearchURL, Int32 Timeout = 15000)
        {

            Console.WriteLine("Searching...");
            WebRequest wrGETURL;

            if (SearchEntry.IsCustomUrl)
            {
                wrGETURL = WebRequest.Create(SearchEntry.URLPrefix + SearchURL + SearchEntry.URLSuffix);
            }
            else
            {
                wrGETURL = WebRequest.Create(SearchEntry.URLPrefix + SearchEntry.URLSuffix);
            }

            wrGETURL.Timeout = Timeout;

            try
            {
                using (WebResponse myWebResponse = wrGETURL.GetResponse())
                {
                    using (Stream objStream = myWebResponse.GetResponseStream())
                    {
                        using (StreamReader objReader = new StreamReader(objStream))
                        {
                            string HttpData = objReader.ReadToEnd();
                            string response = "Invalid Search";
                            string[] SpideredData = HttpData.Split(new string[] { SearchEntry.SpiderPrefix }, StringSplitOptions.RemoveEmptyEntries);

                            if (SpideredData.Length > 1)
                            {
                                SpideredData = SpideredData[1].Split(new string[] { SearchEntry.SpiderSuffix }, StringSplitOptions.RemoveEmptyEntries);
                                response = SpideredData[0];
                            }

                            // Try to avoid console writing and prefer a proper logging system
                            Console.WriteLine(response);

                            return response;
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error when searching: {0}", ex));
                return "An error occured when searching";
            }


        }

    }
}