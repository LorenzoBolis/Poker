using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Client_form
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
        private static Socket server;
        private static Socket chat;
        
        public static string Connetti(string ip_ins)
        {
            try
            {
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ip = IPAddress.Parse(ip_ins);
                IPEndPoint ipEnd = new IPEndPoint(ip, 51000);

                server.Connect(ipEnd);
            }
            catch(Exception ex)
            {
                return "Connessione non riuscita";
            }
            
            byte[] buffer = new byte[1024];
            int received = server.Receive(buffer);
            string response = Encoding.UTF8.GetString(buffer, 0, received);
            if (response == "CONNECTED")
            {
                server.Send(Encoding.UTF8.GetBytes("ACK"));
            }
            if (response == "NOT_CONN")
            {
                return "Connesisone non riuscita";
            }
            return "Connesso al server";
        }
        public static string ConnettiCHAT(string ip_ins)
        {
            try
            {
                chat = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipENndChat = new IPEndPoint(IPAddress.Parse(ip_ins), 51001);
                chat.Connect(ipENndChat);
                return "CHAT connessa";
            }
            catch(Exception ex)
            {
                return "CHAT non riuscita";
            }
        }
        public static void Invia(string messaggio)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(messaggio);
            server.Send(messageBytes);
            if (messaggio == "exit") server.Shutdown(SocketShutdown.Both);
        }
        public static void InviaCHAT(string messaggio)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(messaggio);
            chat.Send(messageBytes);
            if (messaggio == "exit") chat.Shutdown(SocketShutdown.Both);
        }
        public static string Ricevi()
        {
            do
            {
                byte[] buffer = new byte[1024];
                int received = server.Receive(buffer);
                string response = Encoding.UTF8.GetString(buffer, 0, received);
                return response;
            }
            while (true);
        }

        public static async Task<string> RiceviAsync()
        {
            do
            {
                byte[] buffer = new byte[1024];
                int received = await server.ReceiveAsync(buffer, SocketFlags.None);
                string response = Encoding.UTF8.GetString(buffer, 0, received);
                return response;
            }
            while (true);
        }
        public static string RiceviCHAT()
        {
            do
            {
                byte[] buffer = new byte[1024];
                
                int received = chat.Receive(buffer);
                string response = Encoding.UTF8.GetString(buffer, 0, received);
                if (response.Split("|")[0]=="*CHAT*") return response;
            }
            while (true);
        }
        public static string Trova_ip()
        {
            string hostName = Dns.GetHostName();
            IPAddress[] localIPs = Dns.GetHostAddresses(hostName);
            IPAddress mio_ip = IPAddress.Any;
            foreach (IPAddress ipAddress in localIPs)
            {
                if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    mio_ip = ipAddress;
                }
            }
            return mio_ip.ToString();
        }
    }
}