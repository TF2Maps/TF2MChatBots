using System;
using System.Linq;

namespace SteamBotLite
{
    public partial class MapModule
    {
        // The commands
        private class InsertMap : MapCommand
        {
            public InsertMap(ModuleHandler bot, MapModule mapModule) : base(bot, "!insert", mapModule, "!insert <index> <filename> <url> <notes>")
            {
            }

            public override string runcommand(MessageEventArgs msg, string param)
            {
                string[] parameters = param.Split(new char[] { ' ' }, 3);
                int index;

                if (parameters[0].Length == 0)
                {
                    return "Invalid parameters for !insert. Syntax: !insert <index> <filename> <url> <notes>";
                }
                try
                {
                    index = int.Parse(parameters[0]);
                }
                catch
                {
                    return "Invalid parameters for !insert. Syntax: !insert <index> <filename> <url> <notes>";
                }

                Map map = new Map();
                map.Submitter = msg.Sender.identifier.ToString();

                map.SubmitterName = msg.Sender.DisplayName;

                try
                {
                    map.Filename = parameters[1];
                }
                catch (Exception exception)
                {
                    return string.Format("Your new file name was rejected because: {0}", exception.Message);
                }

                map.Notes = string.Format("Inserted in position {0} by {1} //", index, msg.Sender.identifier.ToString());

                if (MapModule.CheckIfMapIsUploaded(map.Filename)) //Check if the map is uploaded
                {
                    map.DownloadURL = "Uploaded";
                    if (parameters.Length > 1)
                    {
                        map.Notes += parameters.Last();
                    }
                }
                else if (parameters.Length > 2) //If its not uploaded check if a URL was there
                {
                    parameters = param.Split(new char[] { ' ' }, 4);

                    map.DownloadURL = parameters[2];
                    if (parameters.Length > 3)
                    {
                        map.Notes += parameters.Last();
                    }
                }
                else //If a url isn't there lets return an error
                {
                    return "Your map isn't uploaded! Please use include the url with the syntax: !add <filename> <url> (notes)";
                }
                string Reply = string.Format("Map '{0}' added.", map.Filename);

                MapModule.mapList.InsertMap(index, map);

                MapModule.savePersistentData();

                return Reply;
            }
        }
    }
}