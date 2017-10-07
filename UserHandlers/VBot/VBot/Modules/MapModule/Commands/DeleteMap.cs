namespace SteamBotLite
{
    public partial class MapModule
    {
        private class DeleteMaps : MapCommand
        {
            public DeleteMaps(ModuleHandler bot, MapModule mapMod) : base(bot, "!delete", mapMod, "!delete <filename> OR !delete <position> ")
            {
            }

            public override string runcommand(MessageEventArgs Msg, string param)
            {
                string[] parameters = param.Split(new char[] { ' ' }, 2);
                int MapPositionInList = 0;
                Map deletedMap = new Map();

                if (int.TryParse(parameters[0], out MapPositionInList))
                {
                    if ((MapPositionInList > MapModule.mapList.GetSize()) | (MapPositionInList <= 0))
                    {
                        return "That index does not exist! Please use a valid number when deleting maps";
                    }
                    else
                    {
                        MapPositionInList--;
                        deletedMap = MapModule.mapList.GetMap(MapPositionInList);
                    }
                }
                else
                {
                    deletedMap = MapModule.mapList.GetMapByFilename(parameters[0]);
                }

                if (deletedMap == null)
                {
                    return string.Format("Map '{0}' was not found.", parameters[0]);
                }
                else
                {
                    if ((deletedMap.IsOwner(Msg.Sender.identifier)) || (userhandler.admincheck(Msg.Sender)))
                    {
                        string Reason = "Deleted by " + Msg.Sender.DisplayName + " (" + Msg.Sender.identifier + "). ";
                        string ExplicitReason = param.Substring(parameters[0].Length, param.Length - parameters[0].Length);

                        if (!string.IsNullOrWhiteSpace(ExplicitReason))
                        {
                            Reason += "Reason given: " + ExplicitReason;
                        }
                        else
                        {
                            Reason += "No reason given";
                        }

                        userhandler.SendPrivateMessageProcessEvent(new MessageEventArgs(null) { Destination = new User(deletedMap.Submitter, null), ReplyMessage = string.Format("Your map {0} has been deleted from the map list. {1}", deletedMap.Filename, Reason) });

                        MapModule.mapList.RemoveMap(deletedMap, Reason);
                        MapModule.savePersistentData();
                        return string.Format("Map '{0}' DELETED. Sending: {1}", deletedMap.Filename, Reason);
                    }
                    else
                    {
                        return string.Format("You do not have permission to edit map '{0}'.", deletedMap.Filename);
                    }
                }
            }
        }
    }
}