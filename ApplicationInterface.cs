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
        public enum UserAdminStatus { Unknown, Other, False, True };

        public UserIdentifier(object Identification, UserAdminStatus Rank = UserAdminStatus.Unknown , object additionaldata = null, string displayname = "Unknown")
        {
            identifier = Identification;
            UserRank = Rank; //TODO check if this throws an error
            DisplayName = displayname;
            if (additionaldata != null)
            {
                extradata = additionaldata;
            }
            else
            {
                extradata = null;
            }
        }
        public UserAdminStatus UserRank;
        public object identifier;
        public object extradata;
        public string DisplayName;
    }

    public class MessageProcessEventData : EventArgs
    {
        public UserIdentifier Sender;
        public ChatRoomIdentifier Chatroom;
        public string ReceivedMessage;
        public string ReplyMessage;       
    }

        public abstract class ApplicationInterface
    {
        

        public abstract void SendChatRoomMessage(object sender, MessageProcessEventData messagedata);
        public abstract void SendPrivateMessage(object sender, MessageProcessEventData messagedata);

        public void AssignUserHandler(UserHandler userhandler)
        {
            userhandler.ChatRoomJoin += EnterChatRoom;
            userhandler.ChatRoomLeave += LeaveChatroom;
            userhandler.SendChatRoomMessageEvent += SendChatRoomMessage;
            userhandler.SendPrivateMessageEvent += SendPrivateMessage;
            userhandler.SetUsernameEvent += SetUsername;
            userhandler.RebootEvent += Reboot;

        }


        /* I beleive there is no benefit to making this role mandatory
        public abstract void ReceiveChatRoomMessage(ChatRoomIdentifier chatroomidentifier, string Message);
        public abstract void ReceivePrivateMessage(UserIdentifier useridentifier, string Message);
        */

        public event EventHandler<MessageProcessEventData> ChatRoomMessageEvent;
        
        //The event-invoking method that derived classes can override.
        protected virtual void ChatRoomMessageProcessEvent(MessageProcessEventData e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<MessageProcessEventData> handler = ChatRoomMessageEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<MessageProcessEventData> PrivateMessageEvent;

        //The event-invoking method that derived classes can override.
        protected virtual void PrivateMessageProcessEvent(MessageProcessEventData e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<MessageProcessEventData> handler = PrivateMessageEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        event EventHandler<Tuple<UserIdentifier, bool>> ChatMemberInfoEvent;
        //The event-invoking method that derived classes can override.
        protected virtual void ChatMemberInfoProcessEvent(UserIdentifier e , bool isadmin)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<Tuple<UserIdentifier,bool>> handler = ChatMemberInfoEvent;
            if (handler != null)
            {
                handler(this, new Tuple<UserIdentifier,bool>(e,isadmin));
            }
        }

        public event EventHandler AnnounceLoginCompletedEvent;

        protected virtual void AnnounceLoginCompleted()
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler handler = AnnounceLoginCompletedEvent;
            if (handler != null)
            {
                AnnounceLoginCompletedEvent(this , null);
            }
        }

        public abstract void ReceiveChatMemberInfo(UserIdentifier useridentifier, bool AdminStatus);

        public abstract void EnterChatRoom (object sender, ChatRoomIdentifier chatroomidentifier);
        public abstract void LeaveChatroom (object sender, ChatRoomIdentifier chatroomidentifier);

        public abstract void Reboot(object sender, EventArgs e);

        public abstract void SetUsername(object sender, string Username);
        public abstract string GetUsername();

        public abstract string GetOthersUsername(object sender, UserIdentifier user);

        public abstract void tick();

        public string Username
        {
            get
            {
                return GetUsername();
            }

            set
            {
               SetUsername(this, value);
            }
        }
    }
}
