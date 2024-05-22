using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DLL
{
    [DataContract]
    public class Message
    {
        public Message(string text, string from, string to)
        {
            this.Time = DateTime.Now;
            this.From = from;
            this.To = to;
            this.Text = text;
            this.Attachemnt = null;
        }
        [DataMember]
        public DateTime Time { get; set; }

        [DataMember]
        public string From { get; set; }

        [DataMember]
        public string To { get; set; }

        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public byte[] Attachemnt { get; set; }

        [DataMember]
        public string Filename { get; set; }

    }
}
