namespace SteamBotLite
{
    public partial class MapModule
    {
        private class WipeMaps : MapCommand
        {
            private MapModule module;

            public WipeMaps(ModuleHandler bot, MapModule mapMod) : base(bot, "!wipe", mapMod, "!wipe <reason>")
            {
                module = mapMod;
            }

            public override string runcommand(MessageEventArgs Msg, string param)
            {
                if (!string.IsNullOrEmpty(param))
                {
                    module.ClearMapListWithMessage(param);
                    module.savePersistentData();
                    return "The map list has been DELETED.";
                }
                else
                {
                    return "The map list has not been DELETED, you must include a reason! !wipe <reason>";
                }
            }
        }
    }
}