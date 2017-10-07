namespace SteamBotLite
{
    public partial class AdminModule
    {
        private class Rejoin : BaseCommand
        {
            // Command to query if a server is active
            private AdminModule module;

            public Rejoin(UserHandler bot, ModuleHandler modulehandler, AdminModule module) : base(modulehandler, "!Rejoin")
            {
                this.module = module;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                module.userhandler.FireMainChatRoomEvent(UserHandler.ChatroomEventEnum.LeaveChat);
                module.userhandler.FireMainChatRoomEvent(UserHandler.ChatroomEventEnum.EnterChat);
                return "Rejoined!";
            }
        }
    }
}