using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DLL
{
    [DataContract]
    public class User
    {
        public User(string username)
        {
            UserID = Guid.NewGuid().ToString();
            this.Username = username;
        }

        [DataMember]
        public string UserID { get; set; }

        [DataMember]
        public string Username { get; set; }

    }
}
