using System;
using System.Collections.Generic;

namespace SteamBotLite
{
    public class ListHolder : BaseModule
    {
        private HTMLFileFromArrayListiners handler;

        public ListHolder(ModuleHandler handler, HTMLFileFromArrayListiners HTMLHandler, Dictionary<string, Dictionary<string, object>> Jsconfig) : base(handler, Jsconfig)
        {
            this.handler = HTMLHandler;

            commands.Add(new FeatureRequest(handler, this));
        }

        public override string getPersistentData()
        {
            throw new NotImplementedException();
        }

        public override void loadPersistentData()
        {
            throw new NotImplementedException();
        }

        public override void OnAllModulesLoaded()
        {
            //Setup Table

            TableDataValue UsernameHeader = new TableDataValue();
            UsernameHeader.VisibleValue = "Username";
            TableDataValue Message = new TableDataValue();
            Message.VisibleValue = "Message";

            handler.SetTableHeader("Requests", new TableDataValue[] { UsernameHeader, Message });
        }

        private class FeatureRequest : BaseCommand
        {
            private ListHolder module;

            public FeatureRequest(ModuleHandler bot, ListHolder module) : base(bot, "!Request")
            {
                this.module = module;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                if (!param.Equals("!Request"))
                {
                    TableDataValue Username = new TableDataValue();
                    Username.VisibleValue = Msg.Sender.identifier.ToString();
                    TableDataValue Message = new TableDataValue();
                    Message.VisibleValue = param;

                    TableDataValue[] Data = new TableDataValue[] { Username, Message };
                    module.handler.AddWebsiteEntryWithoutLimit("Requests", Data);
                    module.handler.AddWebsiteEntryWithLimit("RequestsWithLimit", Data, 3);

                    return "Added Entry";
                }
                else
                {
                    return "Actually put in something";
                }
            }
        }
    }
}