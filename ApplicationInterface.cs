using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamBotLite
{
    /// <summary>
    /// The identification of the chatroom or channel to send messages to and receive them from
    /// </summary>
    public struct ChatRoomIdentifier
    {
        public ChatRoomIdentifier(object Identification, object additionaldata = null)
        {
            identifier = Identification;
            
            if (additionaldata != null)
            {
                extradata = additionaldata;
            }
            else
            {
                extradata = null;
            }
        }
        public object identifier;
        public object extradata;
    }

    /// <summary>
    /// The identification of the individual users
    /// </summary>
    public struct UserIdentifier
    {
        public UserIdentifier(object Identification, object rank = null , object additionaldata = null)
        {
            identifier = Identification;
            Rank = rank; //TODO check if this throws an error
            if (additionaldata != null)
            {
                extradata = additionaldata;
            }
            else
            {
                extradata = null;
            }
        }
        public object Rank;
        public object identifier;
        public object extradata;
    }
   
    public abstract class ApplicationInterface
    {
        public abstract void SendChatRoomMessage(ChatRoomIdentifier chatroomidentifier, string Message);
        public abstract void SendPrivateMessage(UserIdentifier useridentifier, string Message);

        /* I beleive there is no benefit to making this role mandatory
        public abstract void ReceiveChatRoomMessage(ChatRoomIdentifier chatroomidentifier, string Message);
        public abstract void ReceivePrivateMessage(UserIdentifier useridentifier, string Message);
        */ 
        public abstract void ReceiveChatMemberInfo(UserIdentifier useridentifier, bool AdminStatus);

        public abstract void EnterChatRoom (ChatRoomIdentifier chatroomidentifier);
        public abstract void LeaveChatroom (ChatRoomIdentifier chatroomidentifier);

        public abstract void Reboot();

        public abstract void SetUsername(string Username);
        public abstract string GetUsername();

        public abstract string GetOthersUsername(UserIdentifier user);

        public abstract void tick();

        public string Username
        {
            get
            {
                return GetUsername();
            }

            set
            {
               SetUsername(value);
            }
        }
    }
}
