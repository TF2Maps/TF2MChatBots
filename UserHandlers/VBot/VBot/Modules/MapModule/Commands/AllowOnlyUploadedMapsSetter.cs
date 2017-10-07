using System.Linq;

namespace SteamBotLite
{
    public partial class MapModule
    {
        private sealed class AllowOnlyUploadedMapsSetter : BaseCommand
        {
            private MapModule mapmodule;

            public AllowOnlyUploadedMapsSetter(ModuleHandler bot, MapModule module) : base(bot, "!forceuploaded")
            {
                mapmodule = module;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                string[] parameters = param.Split(new char[] { ' ' }, 2);
                bool AllowOnlyUploadedMaps = bool.Parse(parameters[0]);
                if (parameters.Count() != 2 && AllowOnlyUploadedMaps)
                {
                    return "The syntax is: !forceuploaded true/false <reason>";
                }
                if (AllowOnlyUploadedMaps == false)
                {
                    mapmodule.mapList.RestrictMapsToBeUploaded(AllowOnlyUploadedMaps, "");
                }
                else
                {
                    string RejectUnUploadedMapsReply = parameters[1];
                    mapmodule.mapList.RestrictMapsToBeUploaded(AllowOnlyUploadedMaps, RejectUnUploadedMapsReply);
                }
                mapmodule.savePersistentData();

                return string.Format("Config has been updated, forcing maps to be uploaded has been set to: {0} with an error msg: {1}", mapmodule.mapList.AllowOnlyUploadedMaps.ToString(), mapmodule.mapList.ForceMapsToBeUploadedErrorResponse);
            }
        }
    }
}