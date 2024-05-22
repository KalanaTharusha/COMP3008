using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using ChatServerInterface;

namespace ChatServer
{
    internal class Program
    {

        static void Main(string[] args)
        {

            Console.WriteLine("Chat server started");

            ServiceHost host = new ServiceHost(typeof(ChatService));
            NetTcpBinding netTcpBinding = new NetTcpBinding();
            netTcpBinding.MaxReceivedMessageSize = 200_000_000;

            XmlDictionaryReaderQuotas readerQuotas = new System.Xml.XmlDictionaryReaderQuotas();
            readerQuotas.MaxArrayLength = 200_000_000;
            netTcpBinding.ReaderQuotas = readerQuotas;

            host.AddServiceEndpoint(typeof(IChatService), netTcpBinding, "net.tcp://localhost:8000/ChatService");
            var behavior = new ServiceMetadataBehavior { HttpGetEnabled = false };
            host.Description.Behaviors.Add(behavior);
            host.Open();

            Console.WriteLine("Chat Server Online");
            Console.ReadLine();

            host.Close();
        }
    }
}
