namespace SteamBotLite
{
    public class MapValidityCheck
    {
        public bool IsValid = true;
        public string ReturnMessage;

        public void SetInvalid(string message)
        {
            IsValid = false;
            ReturnMessage = message;
        }
    }
}