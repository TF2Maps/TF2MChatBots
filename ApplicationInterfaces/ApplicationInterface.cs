using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace SteamBotLite
{
    public class ChatroomEntity
    {
        public enum AdminStatus { Unknown, Other, False, True };

        public ApplicationInterface Application;
        public object identifier;
        public AdminStatus Rank = AdminStatus.Unknown;
        public string DisplayName = "";
        public object ExtraData;
        public object ParentIdentifier;
        public string UserURL;

        private bool IsChild;

        public ChatroomEntity(object identifier, ApplicationInterface Application)
        {
            this.identifier = identifier;
            this.Application = Application;
        }

        public bool UserEquals(ChatroomEntity OtherEntity)
        {
            return OtherEntity.identifier.ToString().Equals(this.identifier.ToString());
        }

        public bool UserEquals(string OtherUserIdentifier)
        {
            return this.identifier.ToString().Equals(OtherUserIdentifier);
        }
    }

    public class Chatroom : ChatroomEntity
    {
        public Chatroom(object identifier, ApplicationInterface Application) : base(identifier, Application)
        { }
    }

    public class User : ChatroomEntity
    {
        public User(object identifier, ApplicationInterface Application) : base(identifier, Application)
        { }
    };

    public class MessageEventArgs : EventArgs
    {
        public ChatroomEntity Sender;
        public ChatroomEntity Destination;
        public ChatroomEntity Chatroom;
        public string ReceivedMessage;
        public string ReplyMessage;
        public ApplicationInterface InterfaceHandlerDestination;

        public MessageEventArgs(ApplicationInterface interfacehandlerdestination)
        {
            InterfaceHandlerDestination = interfacehandlerdestination;
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }

    public abstract class ApplicationInterface
    {
        public virtual List<ChatroomEntity> GetMainChatroomsCollection()
        {
            List<ChatroomEntity> Chatrooms = new List<ChatroomEntity>();
            foreach (string item in Whitelist)
            {
                Chatrooms.Add(new ChatroomEntity(item, this));
            }
            return Chatrooms;
        }

        public Dictionary<string, object> config;

        public List<string> Whitelist;
        public List<string> Blacklist;
        private bool WhitelistOnly;

        public List<string> MessagingList;

        public ApplicationInterface()
        {
            this.config = JsonConvert.DeserializeObject<Dictionary<string, object>>(System.IO.File.ReadAllText(Path.Combine("applicationconfigs", this.GetType().Name.ToString() + ".json")));

            Whitelist = JsonConvert.DeserializeObject<List<string>>(config["Whitelist"].ToString());
            Blacklist = JsonConvert.DeserializeObject<List<string>>(config["BlackList"].ToString());
            WhitelistOnly = bool.Parse(config["WhitelistOnly"].ToString());
        }

        public bool CheckEntryValid(string entry)
        {
            if (WhitelistOnly)
            {
                if (Whitelist.Contains(entry))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (Blacklist.Contains(entry))
            {
                return false;
            }

            return true;
        }

        public enum TickThreadState { Running, Stopped };

        public TickThreadState TickThread = TickThreadState.Running;

        public abstract void tick();

        public void StartTickThreadLoop()
        {
            while (TickThread == TickThreadState.Running)
            {
                tick();
            }
        }

        public abstract void Reboot(object sender, EventArgs e);

        public abstract void SendChatRoomMessage(object sender, MessageEventArgs messagedata);

        public abstract void SendPrivateMessage(object sender, MessageEventArgs messagedata);

        public abstract void BroadCastMessage(object sender, string message);

        public void AssignUserHandler(UserHandler userhandler)
        {
            userhandler.ChatRoomJoin += EnterChatRoom;
            userhandler.ChatRoomLeave += LeaveChatroom;
            userhandler.SendChatRoomMessageEvent += SendChatRoomMessage;
            userhandler.SendPrivateMessageEvent += SendPrivateMessage;
            userhandler.SetUsernameEvent += SetUsername;
            userhandler.RebootEvent += Reboot;
            userhandler.BroadcastMessageEvent += BroadCastMessage;
            userhandler.MainChatRoomJoin += JoinAllChatrooms;
            userhandler.MainChatRoomLeave += LeaveAllChatrooms;
            userhandler.ChatMemberInfoEvent += ChatMemberInfoEvent;
            userhandler.SetStatusmessage += SetStatusMessage;
        }

        public class ChatMemberInfoEventArgs
        {
            private ChatroomEntity User;
            private bool IsAdmin;
        }

        public event EventHandler<MessageEventArgs> ChatRoomMessageEvent;

        //The event-invoking method that derived classes can override.
        protected virtual void ChatRoomMessageProcessEvent(MessageEventArgs e)
        {
            EventHandler<MessageEventArgs> handler = ChatRoomMessageEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<MessageEventArgs> PrivateMessageEvent;

        //The event-invoking method that derived classes can override.
        protected virtual void PrivateMessageProcessEvent(MessageEventArgs e)
        {
            EventHandler<MessageEventArgs> handler = PrivateMessageEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<Tuple<ChatroomEntity, bool>> ChatMemberInfoEvent;

        protected virtual void ChatMemberInfoProcessEvent(ChatroomEntity e, bool isadmin)
        {
            EventHandler<Tuple<ChatroomEntity, bool>> handler = ChatMemberInfoEvent;
            if (handler != null)
            {
                handler(this, new Tuple<ChatroomEntity, bool>(e, isadmin));
            }
        }

        public event EventHandler AnnounceLoginCompletedEvent;

        protected virtual void AnnounceLoginCompleted()
        {
            EventHandler handler = AnnounceLoginCompletedEvent;
            if (handler != null)
            {
                AnnounceLoginCompletedEvent(this, null);
            }
        }

        public event EventHandler<string> SetStatusMessageEvent;

        public abstract void SetStatusMessage(object sender, string message);

        public abstract void ReceiveChatMemberInfo(ChatroomEntity ChatroomEntity, bool AdminStatus);

        public abstract void EnterChatRoom(object sender, ChatroomEntity ChatroomEntity);

        public abstract void LeaveChatroom(object sender, ChatroomEntity ChatroomEntity);

        public void JoinAllChatrooms(object sender, EventArgs e)
        {
            foreach (ChatroomEntity entry in GetMainChatroomsCollection())
            {
                EnterChatRoom(sender, entry);
            }
        }

        public void LeaveAllChatrooms(object sender, EventArgs e)
        {
            foreach (ChatroomEntity entry in GetMainChatroomsCollection())
            {
                LeaveChatroom(sender, entry);
            }
        }

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

        public abstract string GetUsername();

        public abstract void SetUsername(object sender, string Username);

        public abstract string GetOthersUsername(object sender, ChatroomEntity user);
    }
}