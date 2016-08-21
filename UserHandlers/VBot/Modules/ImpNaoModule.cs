
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using SteamKit2;
using Newtonsoft.Json;

namespace SteamBotLite
{
    class ImpNaoModule : BaseModule
    {
        
        public ImpNaoModule(VBot bot, Dictionary<string, object> Jsconfig) : base(bot, Jsconfig)
        {
            loadPersistentData();
            /*
            commands.Add(new Add(bot, this));
            commands.Add(new Maps(bot, this));
            commands.Add(new Update(bot, this));
            commands.Add(new Delete(bot, this));
            adminCommands.Add(new Wipe(bot, this));
            */
        }

        public override string getPersistentData()
        {
            return null;
        }

        public override void loadPersistentData()
        {
        }

        public void HandleEvent(object sender, ServerModule.ServerInfo args)
        {
        }

        // The abstract command for motd

        abstract public class MapCommand : BaseCommand
        {
            protected ImpNaoModule ImpNaoModule;

            public MapCommand(VBot bot, string command, ImpNaoModule mapMod)
                : base(bot, command)
            {
                this.ImpNaoModule = mapMod;
            }
        }
        

        // The commands

        private class Add : MapCommand
        {
            ImpNaoModule impnaomodule;
            public bool uploadcheck(string MapName, string Website)
            {
                return SearchClass.CheckDataExistsOnWebPage(Website, MapName); //TODO develop method to check website
            }

            public Add(VBot bot, ImpNaoModule Impnaomodule) : base(bot, "!imp", Impnaomodule) {
                impnaomodule = Impnaomodule;
            }

            protected override string exec(SteamID sender, string param)
            {
                impnaomodule.PostAsync("testmap", "Body");
                return "Completed";
            }
        }

        public async Task PostAsync(string Map,string Body)
        {
            using (var client = new HttpClient())
            {

                var values = new Dictionary<string, string>
                        {
                            { "name", "hello" },
                            { "link", "world" },
                            { "password", "elpsykongeroo"}
                        };
                string twent = JsonConvert.SerializeObject(values);

                HttpContent Content = new StringContent(JsonConvert.SerializeObject(values));


                var response = await client.PostAsync("http://carbidegames.com/impnao/api/maps", Content);

                var responseString = await response.Content.ReadAsStringAsync();
            }
        }

        private class Maps : MapCommand
        {
            public Maps(VBot bot, ImpNaoModule impnaomodule) : base(bot, "!maps", impnaomodule) { }
            protected override string exec(SteamID sender, string param)
            {

                return "Completed";
            }
        }

        private class Update : MapCommand
        {
            public Update(VBot bot, ImpNaoModule impnaomodule) : base(bot, "!update", impnaomodule) { }
            protected override string exec(SteamID sender, string param)
            {

                return "Completed";
            }
        }

        private class Delete : MapCommand
        {
            public Delete(VBot bot, ImpNaoModule impnaomodule) : base(bot, "!delete", impnaomodule) { }
            protected override string exec(SteamID sender, string param)
            {

                return "Completed";
            }

        }

        private class Wipe : MapCommand
        {
            public Wipe(VBot bot, ImpNaoModule impnaomodule) : base(bot, "!wipe", impnaomodule) { }
            protected override string exec(SteamID sender, string param)
            {

                return "Completed";
            }
        }
    }
}
