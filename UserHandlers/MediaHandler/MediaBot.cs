using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace SteamBotLite
{
    public class MediaBot : UserHandler
    {
        private string APICommand = "!YoutubeAPIKEY";
        private string ApiKey;

        private string ApiKeySaveFile = "Media.txt";

        public MediaBot()
        {
            GetVideoData("xrbrQhpvn8E");
            ApiKey = GetConfig();
        }

        public override void ChatMemberInfo(object sender, Tuple<ChatroomEntity, bool> e)
        {
        }

        public override void OnLoginCompleted(object sender, EventArgs e)
        {
        }

        public override void ProcessChatRoomMessage(object sender, MessageEventArgs e)
        {
            string[] SplitMessage = e.ReceivedMessage.Split(null);
            foreach (string Word in SplitMessage) //We do this to handle multiple videos
            {
                string VideoID = ExtractID(Word);

                if (string.IsNullOrEmpty(VideoID))
                {
                    //Do Nothing
                }
                else
                {
                    string VideoData = GetVideoData(VideoID);
                    if (string.IsNullOrEmpty(VideoData))
                    {
                        //Do Nothing
                    }
                    else
                    {
                        string item = VideoData;//.Replace("\n", string.Empty);
                        string TimeString;

                        Console.WriteLine(item);

                        dynamic red = JsonConvert.DeserializeObject(item);
                        RootObject data = JsonConvert.DeserializeObject<RootObject>(item);

                        string time = data.items[0].contentDetails.duration;

                        if (time.Contains("D"))
                        {
                            TimeString = "Over 24 Hours long";
                        }
                        else
                        {
                            TimeString = XmlConvert.ToTimeSpan(time).ToString();
                        }

                        string Hoursheader = "00:";
                        if (TimeString.StartsWith(Hoursheader))
                        {
                            TimeString = TimeString.Substring(Hoursheader.Length, TimeString.Length - Hoursheader.Length);
                        }
                        if (TimeString.Equals("00:00"))
                        {
                            TimeString = "Stream";
                        }
                        e.ReplyMessage = data.items[0].snippet.title + " [" + TimeString + "]";
                        e.InterfaceHandlerDestination.SendChatRoomMessage(this, e);
                    }
                    Console.WriteLine(GetVideoData(VideoID));
                }
            }
        }

        public override void ProcessPrivateMessage(object sender, MessageEventArgs e)
        {
            e.InterfaceHandlerDestination.SendPrivateMessage(this, e);
            if (e.ReceivedMessage.StartsWith(APICommand, StringComparison.OrdinalIgnoreCase))
            {
                int StartIndex = APICommand.Length;
                int CharactersRemaining = e.ReceivedMessage.Length - APICommand.Length;

                string Key = e.ReceivedMessage.Substring(StartIndex, CharactersRemaining);
                SetApiKey(Key);
                e.ReplyMessage = "Retrieved Key";
                e.InterfaceHandlerDestination.SendPrivateMessage(this, e);
            }
        }

        private string ExtractID(string Message)
        {
            string[] YoutubeRepresentations = new string[] {
                "https://youtu.be/",
                "https://www.youtube.com/watch?v=",
                "https://www.m.youtube.com/watch?v="
            };

            for (int i = 0; i < YoutubeRepresentations.Length; i++)
            {
                if (Message.ToLower().Contains(YoutubeRepresentations[i]))
                {
                    string Value = TrimOpeningForURL(Message, YoutubeRepresentations[i].Length);

                    Value = TrimEnding(Value);

                    if (Value.EndsWith("/"))
                    {
                        Value = Value.Substring(0, Value.Length - 1);
                    }
                    if (Value.Contains("?t="))
                    {
                        Value = Value.Split(new string[] { "?t=" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    }

                    return Value;
                }
            }
            return null;
        }

        private string ExtractURL(string Message)
        {
            return Message;
        }

        private string GetConfig()
        {
            if (File.Exists(ApiKeySaveFile))
            {
                return System.IO.File.ReadAllText(@ApiKeySaveFile);
            }
            else
            {
                Console.WriteLine("API Functionality will be disabled until a user sends the command: " + APICommand + " <Key>");
                return null;
            }
        }

        private string GetVideoData(string ID)
        {
            try
            {
                string header = "https://www.googleapis.com/youtube/v3/videos?id=";

                string Key = "&key=" + ApiKey + "";
                string trailer = "&fields=items(contentDetails,snippet(title))&part=snippet,contentDetails";
                string WebData = SearchClass.GetWebPageAsString(header + ID + Key + trailer);
                Console.WriteLine(WebData);
                return WebData;
            }
            catch
            {
                return null;
            }
        }

        private void SetApiKey(string value)
        {
            System.IO.File.WriteAllText(ApiKeySaveFile, value);
            ApiKey = value;
        }

        private string TrimEnding(string Message)
        {
            return Message.Split(null)[0];
        }

        private string TrimOpeningForURL(string MainString, int Trimmer)
        {
            int StartIndex = Trimmer;
            int CharactersRemaining = MainString.Length - Trimmer;
            return MainString.Substring(StartIndex, CharactersRemaining);
        }

        public class ContentDetails
        {
            public string duration { get; set; }
        }

        public class Item
        {
            public ContentDetails contentDetails { get; set; }
            public Snippet snippet { get; set; }
        }

        public class RegionRestriction
        {
            public List<string> allowed { get; set; }
        }

        public class RootObject
        {
            public List<Item> items { get; set; }
        }

        public class Snippet
        {
            public string title { get; set; }
        }
    }
}