namespace SteamBotLite
{
    public partial class MapModule
    {
        private sealed class UploadCheck : MapCommand
        {
            public UploadCheck(ModuleHandler bot, MapModule mapModule) : base(bot, "!uploadcheck", mapModule, "!uploadcheck <mapname>")
            { }

            public override string runcommand(MessageEventArgs Msg, string param)
            {
                return MapModule.CheckIfMapIsUploaded(param).ToString();
            }
        }
    }
}