using System.Collections.Specialized;

namespace SteamBotLite
{
    public partial class MapModule
    {
        private sealed class UpdateName : BaseCommand
        {
            private MapModule mapmodule;

            public UpdateName(ModuleHandler bot, MapModule module) : base(bot, "!nameupdate")
            {
                mapmodule = module;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                userhandler.OnMaplistchange(mapmodule.mapList.GetAllMaps(), Msg, args);
                return "Name has been updated";
            }
        }
    }
}