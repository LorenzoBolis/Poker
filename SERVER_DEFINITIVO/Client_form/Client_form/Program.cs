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
        
        public static string Connetti(string ip_ins)
        {
            try
            {
                // Initialize socket
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ip = IPAddress.Parse(ip_ins);
                IPEndPoint ipEnd = new IPEndPoint(ip, 51000);

                // Connect to server
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
        public static void Invia(string messaggio)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(messaggio);
            server.Send(messageBytes);
            if (messaggio == "exit") server.Shutdown(SocketShutdown.Both);
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

    }
}