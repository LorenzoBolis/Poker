using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


class Program  // CLIENT
{
    [STAThread]
    static void Main()
    {
        
        // Initialize socket
        Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ip = IPAddress.Parse("192.168.1.139");
        IPEndPoint ipEnd = new IPEndPoint(ip, 9000);

        // Connect to server
        client.Connect(ipEnd);

        // riceve conferma connessione
        byte[] buffer = new byte[1024];
        int received = client.Receive(buffer);
        string response = Encoding.UTF8.GetString(buffer, 0, received);
        if (response == "CONNECTED")
        {
            Console.WriteLine("Connesso al server");
            client.Send(Encoding.UTF8.GetBytes("ACK"));
        }
        if (response == "NOT_CONN")
        {
            Console.WriteLine("Connesisone non riuscita");
            Console.ReadLine();
            Environment.Exit(0);
        }

        Thread thinvia = new Thread(() =>
        {
            Invia(client);
        });
        thinvia.Start();

        Thread thricevi = new Thread(() =>
        {
            Ricevi(client);
        });
        thricevi.Start();

    }
    static void Invia(Socket client)
    {
        string messaggio;
        do
        {
            messaggio = Console.ReadLine();
            byte[] messageBytes = Encoding.UTF8.GetBytes(messaggio);
            client.Send(messageBytes);
            if (messaggio == "exit") client.Shutdown(SocketShutdown.Both);
        }
        while (messaggio != "exit");
    }
    static void Ricevi(Socket client)
    {
        do
        {
            byte[] buffer = new byte[1024];
            int received = client.Receive(buffer);
            string response = Encoding.UTF8.GetString(buffer, 0, received);
            Console.WriteLine(response);
        }
        while (true);
    }
}