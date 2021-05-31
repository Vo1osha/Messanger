using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    [Serializable]
    public class Message
    {
        public User Sender { set; get; }
        public User Reciever { set; get; }
        public bool Broadcast { set; get; }
        public string MessageString { set; get; }
    }
}
