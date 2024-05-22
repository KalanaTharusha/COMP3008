using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ChatServerInterface
{
    [DataContract]
    public class ServerFault
    {
        private string message;

        [DataMember]
        public string Message
        {
            get { return message; }
            set { message = value; }
        }
    }
}
