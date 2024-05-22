using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using DLL;
using System.Collections.ObjectModel;

namespace ChatServerInterface
{
    [ServiceContract(CallbackContract = typeof(IChatCallback))]
    public interface IChatService
    {

        [OperationContract]
        [FaultContract(typeof(ServerFault))]
        void ConnectUser(User user);

        [OperationContract]
        void JoinChatRoom(string roomName, User user);

        [OperationContract]
        void ExitChatRoom(string roomName, User user);

        [OperationContract]
        void CreateChatRoom(string roomName);

        [OperationContract]
        List<string> getParticipants(string roomName);

        [OperationContract]
        void SendMessage(string roomName, Message message);

        [OperationContract]
        ObservableCollection<string> getChatRooms();

        [OperationContract]
        void DisconnectUser(User user);
    }
}
