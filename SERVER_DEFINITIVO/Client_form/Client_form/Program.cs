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
        static Socket client;
        public static string Connetti()
        {
            try
            {
                // Initialize socket
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ip = IPAddress.Parse("192.168.36.83");
                IPEndPoint ipEnd = new IPEndPoint(ip, 9000);

                // Connect to server
                client.Connect(ipEnd);
            }
            catch(Exception ex)
            {
                return "Connessione non riuscita";
            }
            

            // riceve conferma connessione
            byte[] buffer = new byte[1024];
            int received = client.Receive(buffer);
            string response = Encoding.UTF8.GetString(buffer, 0, received);
            if (response == "CONNECTED")
            {
                client.Send(Encoding.UTF8.GetBytes("ACK"));
            }
            if (response == "NOT_CONN")
            {
                return "Connesisone non riuscita";
            }

            /*Thread thinvia = new Thread(() =>
            {
                Invia(client);
            });
            thinvia.Start();*/

            /*Thread thricevi = new Thread(() =>
            {
                Ricevi(client);
            });
            thricevi.Start();*/
            return "Connesso al server";
        }
        public static void Invia(string messaggio)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(messaggio);
            client.Send(messageBytes);
            if (messaggio == "exit") client.Shutdown(SocketShutdown.Both);
        }
        public static string Ricevi()
        {
            do
            {
                byte[] buffer = new byte[1024];
                int received = client.Receive(buffer);
                string response = Encoding.UTF8.GetString(buffer, 0, received);
                return response;
            }
            while (true);
        }

    }
}