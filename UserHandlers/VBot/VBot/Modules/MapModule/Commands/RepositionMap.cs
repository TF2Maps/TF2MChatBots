namespace SteamBotLite
{
    public partial class MapModule
    {
        private class RepositionMap : MapCommand
        {
            public RepositionMap(ModuleHandler bot, MapModule mapMod) : base(bot, "!reposition", mapMod, "!reposition <new position> <filename>")
            {
            }

            public override string runcommand(MessageEventArgs Msg, string param)
            {
                string[] parameters = param.Split(' ');

                if (parameters.Length < 2)
                {
                    return string.Format("Invalid parameters for !reposition. Syntax: !reposition <new position> <filename>");
                }
                else
                {
                    int index;
                    try
                    {
                        index = int.Parse(parameters[0]);
                    }
                    catch
                    {
                        return string.Format("Invalid parameters for !reposition. Syntax: !reposition <new position> <filename>");
                    }
                    Map editedMap = null;
                    foreach (Map entry in MapModule.mapList)
                    {
                        if (entry.Filename == parameters[1])
                            editedMap = entry;
                    }

                    if (editedMap == null)
                    {
                        return "Map not found";
                    }

                    // Map editedMap = MapModule.mapList.Find(map => map.filename.Equals(parameters[0])); //OLD Map CODE
                    if (editedMap.Submitter.Equals(Msg.Sender.identifier.ToString()) | (userhandler.admincheck(Msg.Sender)))
                    {
                        MapModule.mapList.RemoveMap(editedMap, "Map Repositioned");
                        editedMap.Notes += string.Format("Map repositioned to {0} by {1} // ", index, Msg.Sender.identifier.ToString());
                        MapModule.mapList.InsertMap(index, editedMap);
                        MapModule.savePersistentData();
                        return string.Format("Map '{0}' has been repositioned to {1}.", editedMap.Filename, index);
                    }
                    else
                    {
                        return string.Format("You cannot edit map '{0}' as you did not submit it.", editedMap.Filename);
                    }
                }
            }
        }
    }
}