using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace SteamBotLite
{
    public abstract class ApplicationInterface
    {
        public List<string> Blacklist;

        public Dictionary<string, object> config;

        public List<string> MessagingList;

        public TickThreadState TickThread = TickThreadState.Running;

        public List<string> Whitelist;

        private bool WhitelistOnly;

        public ApplicationInterface()
        {
            this.config = JsonConvert.DeserializeObject<Dictionary<string, object>>(System.IO.File.ReadAllText(Path.Combine("applicationconfigs", this.GetType().Name.ToString() + ".json")));

            Whitelist = JsonConvert.DeserializeObject<List<string>>(config["Whitelist"].ToString());
            Blacklist = JsonConvert.DeserializeObject<List<string>>(config["BlackList"].ToString());
            WhitelistOnly = bool.Parse(config["WhitelistOnly"].ToString());
        }

        public event EventHandler AnnounceLoginCompletedEvent;

        public event EventHandler<Tuple<ChatroomEntity, bool>> ChatMemberInfoEvent;

        public event EventHandler<MessageEventArgs> ChatRoomMessageEvent;

        public event EventHandler<MessageEventArgs> PrivateMessageEvent;

        public event EventHandler<string> SetStatusMessageEvent;

        public enum TickThreadState { Running, Stopped };

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

        public abstract void BroadCastMessage(object sender, string message);

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

        public abstract void EnterChatRoom(object sender, ChatroomEntity ChatroomEntity);

        public virtual List<ChatroomEntity> GetMainChatroomsCollection()
        {
            List<ChatroomEntity> Chatrooms = new List<ChatroomEntity>();
            foreach (string item in Whitelist)
            {
                Chatrooms.Add(new ChatroomEntity(item, this));
            }
            return Chatrooms;
        }

        public abstract string GetOthersUsername(object sender, ChatroomEntity user);

        public abstract string GetUsername();

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

        public abstract void LeaveChatroom(object sender, ChatroomEntity ChatroomEntity);

        public abstract void Reboot(object sender, EventArgs e);

        public abstract void ReceiveChatMemberInfo(ChatroomEntity ChatroomEntity, bool AdminStatus);

        public abstract void SendChatRoomMessage(object sender, MessageEventArgs messagedata);

        public abstract void SendPrivateMessage(object sender, MessageEventArgs messagedata);

        public abstract void SetStatusMessage(object sender, string message);

        public abstract void SetUsername(object sender, string Username);

        public void StartTickThreadLoop()
        {
            while (TickThread == TickThreadState.Running)
            {
                System.Threading.Thread.Sleep(250);

                try {
                    tick();
                } catch (Exception e)
                { }
            }
        }

        public abstract void tick();

        protected virtual void AnnounceLoginCompleted()
        {
            EventHandler handler = AnnounceLoginCompletedEvent;
            if (handler != null)
            {
                AnnounceLoginCompletedEvent(this, null);
            }
        }

        protected virtual void ChatMemberInfoProcessEvent(ChatroomEntity e, bool isadmin)
        {
            EventHandler<Tuple<ChatroomEntity, bool>> handler = ChatMemberInfoEvent;
            if (handler != null)
            {
                handler(this, new Tuple<ChatroomEntity, bool>(e, isadmin));
            }
        }

        //The event-invoking method that derived classes can override.
        protected virtual void ChatRoomMessageProcessEvent(MessageEventArgs e)
        {
            EventHandler<MessageEventArgs> handler = ChatRoomMessageEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        //The event-invoking method that derived classes can override.
        protected virtual void PrivateMessageProcessEvent(MessageEventArgs e)
        {
            EventHandler<MessageEventArgs> handler = PrivateMessageEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public class ChatMemberInfoEventArgs
        {
            private bool IsAdmin;
            private ChatroomEntity User;
        }
    }

    public class Chatroom : ChatroomEntity
    {
        public Chatroom(object identifier, ApplicationInterface Application) : base(identifier, Application)
        { }
    }

    public class ChatroomEntity
    {
        public ApplicationInterface Application;

        public string DisplayName = "";

        public object ExtraData;

        public object identifier;

        public object ParentIdentifier;

        public AdminStatus Rank = AdminStatus.Unknown;

        public string UserURL;

        private bool IsChild;

        public ChatroomEntity(object identifier, ApplicationInterface Application)
        {
            this.identifier = identifier;
            this.Application = Application;
        }

        public enum AdminStatus { Unknown, Other, False, True };

        public bool UserEquals(ChatroomEntity OtherEntity)
        {
            return OtherEntity.identifier.ToString().Equals(this.identifier.ToString());
        }

        public bool UserEquals(string OtherUserIdentifier)
        {
            return this.identifier.ToString().Equals(OtherUserIdentifier);
        }
    }

    public class MessageEventArgs : EventArgs
    {
        public ChatroomEntity Chatroom;
        public ChatroomEntity Destination;
        public ApplicationInterface InterfaceHandlerDestination;
        public string ReceivedMessage;
        public string ReplyMessage;
        public ChatroomEntity Sender;

        public MessageEventArgs(ApplicationInterface interfacehandlerdestination)
        {
            InterfaceHandlerDestination = interfacehandlerdestination;
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }

    public class User : ChatroomEntity
    {
        public User(object identifier, ApplicationInterface Application) : base(identifier, Application)
        { }
    };
}