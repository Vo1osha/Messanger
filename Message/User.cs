using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    [Serializable]
    public class User
    {
        public string Username { set; get; }
        public TcpClient Client { set; get; }
    }
}
