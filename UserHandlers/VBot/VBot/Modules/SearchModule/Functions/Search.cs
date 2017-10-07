namespace SteamBotLite
{
    internal partial class SearchModule
    {
        private class Search : BaseCommand
        {
            private SearchClassEntry SearchData;

            public Search(VBot bot, SearchClassEntry SearchEntry) : base(bot, SearchEntry.Command)
            {
                this.SearchData = SearchEntry;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                return (SearchClass.Search(SearchData, param));
            }
        }
    }
}