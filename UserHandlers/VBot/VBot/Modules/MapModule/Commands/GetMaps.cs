using System;
using System.Collections.Generic;

namespace SteamBotLite
{
    public partial class MapModule
    {
        private class GetMaps : BaseCommand
        {
            private DateTime LastExecuted;
            private MapModule module;

            public GetMaps(ModuleHandler bot, MapModule mapMod) : base(bot, "!maps")
            {
                module = mapMod;
            }

            private enum MapSearchFilter
            { StartsWith, EndsWith, NoFilter, Contains };

            protected override string exec(MessageEventArgs Msg, string param)
            {
                MapSearchFilter Filter = MapSearchFilter.NoFilter;
                string MapFilter = "";
                //If the param actually has a mapname, and isn't empty, execute the filtering system
                bool ValidData = (string.IsNullOrWhiteSpace(param) != true && param.StartsWith("!maps", StringComparison.OrdinalIgnoreCase) != true);
                bool OnlyWantUploaded = Msg.ReceivedMessage.StartsWith("!mapsuploaded", StringComparison.OrdinalIgnoreCase);

                if (ValidData)
                {
                    char asterisk = '*';
                    bool StartsWithAsterisk = param.StartsWith(asterisk.ToString());
                    bool EndsWithAsterisk = param.EndsWith(asterisk.ToString());

                    param.TrimStart(asterisk);
                    param.TrimEnd(asterisk);

                    if (StartsWithAsterisk)
                    {
                        Filter = MapSearchFilter.EndsWith;
                        param = param.Substring(1, param.Length - 1);
                    }
                    if (EndsWithAsterisk)
                    {
                        param = param.Substring(0, param.Length - 1);
                        Filter = MapSearchFilter.StartsWith;
                    }

                    if (StartsWithAsterisk == EndsWithAsterisk) //Either starts AND ends with asterisks, OR No Asterisks
                    {
                        Filter = MapSearchFilter.Contains;
                    }

                    MapFilter = param;
                }

                Tuple<string, string> Responses = GetMapsWithFilter(MapFilter, Filter, OnlyWantUploaded);

                userhandler.SendPrivateMessageProcessEvent(new MessageEventArgs(null) { Destination = Msg.Sender, ReplyMessage = Responses.Item2 });

                if (DateTime.Now < LastExecuted.AddMinutes(1))
                {
                    userhandler.SendPrivateMessageProcessEvent(new MessageEventArgs(null) { Destination = Msg.Sender, ReplyMessage = Responses.Item1 });

                    return null;
                }
                else
                {
                    LastExecuted = DateTime.Now;
                    return Responses.Item1;
                }
            }

            private Tuple<string, string> GetMapsWithFilter(string Filter, MapSearchFilter FilterType, bool OnlyReturnUploadedMaps)
            {
                IReadOnlyList<Map> maps = module.mapList.GetAllMaps();

                if (maps.Count == 0)
                {
                    return new Tuple<string, string>("The map list is empty", " ");
                }
                else
                {
                    string chatResponse = "";
                    string pmResponse = "";

                    int MapsAddedToResponse = 0;
                    int MapsInResponseLimit = module.MaxMapNumber;

                    //Build Chat Response
                    for (int i = 0; i < maps.Count; i++)
                    {
                        if (OnlyReturnUploadedMaps && (module.CheckIfMapIsUploaded(maps[i].Filename) == false))
                        {
                            // do nothing
                        }
                        else if (MapNamePassesFilter(maps[i].Filename, FilterType, Filter))
                        {
                            if (MapsAddedToResponse < MapsInResponseLimit)
                            {
                                if (MapsAddedToResponse > 0)
                                {
                                    chatResponse += " , ";
                                }

                                chatResponse += maps[i].Filename;
                                MapsAddedToResponse++;
                            }
                            int Nextnum = i + 1;
                            string mapLine = string.Format(Nextnum + ") {0} // {1} // {2} ({3})", maps[i].Filename, maps[i].DownloadURL, maps[i].SubmitterName, maps[i].Submitter.ToString());

                            if (!string.IsNullOrEmpty(maps[i].Notes))
                                mapLine += "\nNotes: " + maps[i].Notes;

                            if (i < maps.Count - 1)
                                mapLine += "\n";

                            pmResponse += mapLine;
                        }
                    }

                    if (maps.Count > MapsAddedToResponse)
                    {
                        chatResponse += string.Format(" (and {0} more at: http://vbot.website )", maps.Count - MapsAddedToResponse);
                    }
                    else
                    {
                        chatResponse += " at: http://vbot.site";
                    }

                    if (MapsAddedToResponse == 0)
                    {
                        return new Tuple<string, string>("There were no maps found with those search terms!", "");
                    }

                    return new Tuple<string, string>(chatResponse, pmResponse);
                }
            }

            private bool MapNamePassesFilter(string MapName, MapSearchFilter FilterType, string Filter)
            {
                switch (FilterType)
                {
                    case MapSearchFilter.StartsWith:
                        return MapName.StartsWith(Filter, StringComparison.CurrentCulture);
                        break;

                    case MapSearchFilter.EndsWith:
                        return MapName.EndsWith(Filter, StringComparison.CurrentCulture);
                        break;

                    case MapSearchFilter.Contains:
                        return MapName.Contains(Filter);
                        break;

                    case MapSearchFilter.NoFilter:
                        return true;
                        break;
                }
                return true; //Why did we arrive here
            }
        }
    }
}
