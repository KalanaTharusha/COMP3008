using ChatServerInterface;
using DLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServerInterface
{
    public class ChatRoomService
    {
        public string roomName;
        private Dictionary<User, IChatCallback> participants = new Dictionary<User, IChatCallback>();
        private List<string> usernames = new List<string>();

        public ChatRoomService(string roomName)
        {
            this.roomName = roomName;
            usernames.Add("All");
        }

        public void AddParticipant(User user, IChatCallback chatCallback)
        {
            participants.Add(user, chatCallback);
            usernames.Add(user.Username);
        }

        public void RemoveParticipant(User user, IChatCallback chatCallback)
        {
            participants.Remove(user);
            usernames.Remove(user.Username);
        }

        public List<string> participantNames()
        {
            return usernames;
        }

        public void BroadcastMessage(Message message)
        {
            if (message.To.Equals("All")) {
                foreach (var participant in participants.Values)
                {
                    participant.ReceiveMessage(message);
                }
            } else
            {
                foreach (var participant in participants)
                {
                    if (participant.Key.Username.Equals(message.To) || participant.Key.Username.Equals(message.From))
                    {
                        participant.Value.ReceiveMessage(message);
                    }
                }
            }
            
        }
    }
}
