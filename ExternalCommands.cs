using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;
using System.Net;

namespace FivemStormManager
{
    static class ExternalCommands
    {
        private static Socket socket;

        public static string BeginReading()
        {
            Socket local_socket = socket.Accept();
            byte[] bytes        = new Byte[1024];
            string data         = null;
                
            while (true)
            {
                data += Encoding.ASCII.GetString(bytes, 0, local_socket.Receive(bytes));
                if (data.IndexOf("<EOF>") > -1)
                    break;
            }
            local_socket.Close();
            return data.Replace("<EOF>", "");
        }

        public static void InitSocket()
        {
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ip_address = host.AddressList[1];
            IPEndPoint end_point = new IPEndPoint(ip_address, 6666);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(end_point);
            socket.Listen(10);
        }
    }
}
