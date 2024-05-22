using ChatServerInterface;
using DLL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    internal class ChatService : IChatService
    {
        public List<string> users = new List<string>();

        private static readonly Dictionary<User, IChatCallback> clients = new Dictionary<User, IChatCallback>();
        private static readonly List<ChatRoomService> chatRooms = new List<ChatRoomService>();

        public ChatService()
        {
            if (chatRooms.Count == 0)
            {
                ChatRoomService cr1 = new ChatRoomService("chat room 1");
                ChatRoomService cr2 = new ChatRoomService("chat room 2");
                chatRooms.Add(cr1);
                chatRooms.Add(cr2);
            }
        }

        public void ConnectUser(User user)
        {
            var callback = OperationContext.Current.GetCallbackChannel<IChatCallback>();

            lock(clients)
            {
                if (checkUsernameAvailability(user.Username))
                {
                    clients.Add(user, callback);
                    users.Add(user.Username);
                    Console.WriteLine(user.Username + " connected");
                }
                else
                {
                    Console.WriteLine($"Invalid username {user.Username}");
                    ServerFault serverFault = new ServerFault();
                    serverFault.Message = $"Invalid username {user.Username}";
                    throw new FaultException<ServerFault>(serverFault, new FaultReason("Invalid username"));
                }
            }
        }

        public void JoinChatRoom(string roomName, User user)
        {
            ChatRoomService chatRoom = chatRooms.FirstOrDefault(cr => cr.roomName == roomName);

            if (chatRoom != null)
            {
                foreach (var client in clients)
                {
                    if (client.Key.Username == user.Username)
                    {
                        chatRoom.AddParticipant(client.Key, client.Value);

                    }

                }
            }

        }

        public void ExitChatRoom(string roomName, User user)
        {
            ChatRoomService chatRoom = chatRooms.FirstOrDefault(cr => cr.roomName == roomName);

            if (chatRoom != null)
            {
                foreach (var client in clients)
                {
                    if (client.Key.Username == user.Username)
                    {
                        chatRoom.RemoveParticipant(client.Key, client.Value);
                    }
                }
            }

        }

        public void SendMessage(string roomName, Message message)
        {
            ChatRoomService chatRoom = chatRooms.FirstOrDefault(cr => cr.roomName == roomName);

            if (chatRoom != null)
            {
                chatRoom.BroadcastMessage(message);
            }

        }

        public void CreateChatRoom(string roomName)
        {
            lock (chatRooms)
            {
                if (checkChatRoomAvailability(roomName))
                {
                    ChatRoomService chatRoom = new ChatRoomService(roomName);
                    Console.WriteLine($"Adding new chat room : {roomName}");
                    chatRooms.Add(chatRoom);
                    foreach (var client in clients)
                    {
                        client.Value.UpdateChatRoomInfo(roomName);
                        Console.WriteLine($"Updating: {client.Key.Username}");
                    }
                }
                else
                {
                    Console.WriteLine($"Invalid room name {roomName}");
                    ServerFault serverFault = new ServerFault();
                    serverFault.Message = $"Invalid room name {roomName}";
                    throw new FaultException<ServerFault>(serverFault, new FaultReason("Invalid room name"));
                }
            }
            Console.WriteLine("Update done");

        }

        public ObservableCollection<string> getChatRooms()
        {
            ObservableCollection<string> roomList = new ObservableCollection<string>();
            foreach (var room in chatRooms)
            {
                if (!roomList.Contains(room.roomName))
                {
                    roomList.Add(room.roomName);
                }
            }

            return roomList;
        }

        public List<string> getParticipants(string roomName)
        {

            foreach (var room in chatRooms)
            {
                if (room.roomName.Equals(roomName))
                {
                    return room.participantNames();
                }
            }

            return null;

        }

        private bool checkUsernameAvailability(string username)
        {
            foreach (var user in clients.Keys)
            {
                if(user.Username.Equals(username))
                {
                    return false;
                }
            }
            return true;
        }

        private bool checkChatRoomAvailability(string roomName)
        {
            foreach (var room in chatRooms)
            {
                if (room.roomName.Equals(roomName))
                {
                    return false;
                }
            }
            return true;
        }

        public void DisconnectUser(User user)
        {
            lock(clients)
            {
                User disconnectedUser = clients.FirstOrDefault(u => u.Key.Username.Equals(user.Username)).Key;
                clients.Remove(disconnectedUser);
            }
            
            lock(users)
            {
                users.Remove(user.Username);
            }

        }
    }
}
