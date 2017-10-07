namespace SteamBotLite
{
    public partial class MapModule
    {
        private class GetOwner : BaseCommand
        {
            private MapModule module;

            public GetOwner(ModuleHandler bot, MapModule mapMod) : base(bot, "!GetOwner")
            {
                module = mapMod;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                string[] parameters = param.Split(' ');

                if (parameters.Length > 0)
                {
                    Map GetMap = module.mapList.GetMapByFilename(parameters[0]);

                    if (GetMap == null)
                    {
                        return string.Format("Map '{0}' was not found.", parameters[0]);
                    }
                    else
                    {
                        return string.Format("Map owner is: {0} Extra info: {1} | {2} | Owner: {3} ", GetMap.Submitter, GetMap.Submitter.ToString(), GetMap.SubmitterName, GetMap.IsOwner(Msg.Sender.identifier));
                    }
                }
                return "Invalid parameters for !GetOwner. Syntax: !GetOwner <filename>";
            }
        }
    }
}