namespace SteamBotLite
{
    public partial class MapModule
    {
        private class UpdateMap : MapCommand
        {
            public UpdateMap(ModuleHandler bot, MapModule mapMod) : base(bot, "!update", mapMod, "!update <Current Filename> <New filename> <New Url> <new notes>")
            {
            }

            public override string runcommand(MessageEventArgs msg, string param)
            {
                string[] parameters = param.Split(new char[] { ' ' }, 2);

                if (parameters.Length > 1)
                {
                    Map NewMapdata = ParseStringToMap(parameters[1]);
                    if (userhandler.admincheck(msg.Sender))
                    {
                        msg.Sender.Rank = ChatroomEntity.AdminStatus.True;
                    }

                    return MapModule.mapList.UpdateMap(parameters[0], NewMapdata, msg.Sender);
                }
                else
                {
                    return string.Format("Invalid parameters for !update. Syntax: !update <Current filename> <New filename> <New Url> <New Notes>");
                }
            }
        }
    }
}