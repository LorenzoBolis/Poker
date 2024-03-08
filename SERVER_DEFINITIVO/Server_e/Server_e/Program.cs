using Server_e;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

class Program  // SERVER - 192.168.1.139  (184 mik)
{
    // ipclient - nomeclient
    private static Dictionary<string, string> dizionario = new Dictionary<string, string>();
    private static List<Socket> handlers = new List<Socket>();
    static bool client1_gioco_a = false, client2_gioco_a = false; // determinano se hanno avviato il gioco

    [STAThread]
    static void Main()
    {

        int contatore = 0;
        

        // Initialize socket
        Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("10.1.0.146"), 9000);

        // Bind socket to IP and port
        listener.Bind(ipEndPoint);
        listener.Listen(4);

        Console.WriteLine("Server in ascolto su {0}", ipEndPoint);

        while (contatore <= 1)
        {
            // Accetta connessioni in arrivo
            Socket handler = listener.Accept();

            // Send message
            string message = "CONNECTED";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            handler.Send(messageBytes);

            byte[] buffer = new byte[1024];
            int received = handler.Receive(buffer);
            string response = Encoding.UTF8.GetString(buffer, 0, received);
            if (response == "ACK")
            {
                contatore++;
                handlers.Add(handler);
                dizionario.Add(handler.RemoteEndPoint.ToString(), "Client" + contatore.ToString());
                Console.WriteLine($"Nuova connessione da {handler.RemoteEndPoint} - {dizionario[handler.RemoteEndPoint.ToString()]}");

                // Crea un thread separato per gestire la connessione
                Thread clientThread = new Thread(() =>
                {
                    HandleClient(handler);
                });
                clientThread.Start();
            }

            
        }
        while (true)
        {
            Socket nooo = listener.Accept();
            nooo.Send(Encoding.UTF8.GetBytes("NOT_CONN"));
            nooo.Close();
        }
    }

    static void HandleClient(Socket handler)
    {
        string name = dizionario[handler.RemoteEndPoint.ToString()];
        try
        {
            string message = "";
            // Gestisci il flusso di dati con il client
            do
            {
                byte[] received = new byte[1024];
                int receivedBytes = handler.Receive(received);
                message = Encoding.UTF8.GetString(received, 0, receivedBytes);
                if (message == "exit")
                {
                    Console.WriteLine($"Chiusura connessione con {name}");
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                    break;
                }
                else if (message == "GAME")
                {
                    if (name == "Client1")
                    {
                        client1_gioco_a = true;
                    }
                    else if (name == "Client2")
                    {
                        client2_gioco_a = true;
                    }
                    
                    Invia_Start();
                }
                Console.WriteLine($"Dati ricevuti da {name}: {message}");
            }
            while (message != "exit");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Errore nella gestione del client: " + ex.Message);
        }
    }
    static void Invia_Start()
    {
        string ackMessage = "game_started";
        byte[] ackBytes = Encoding.UTF8.GetBytes(ackMessage);
        if (client1_gioco_a && client2_gioco_a)
        {
            try
            {
                handlers[1].Send(ackBytes);
                handlers[0].Send(ackBytes);
                Gioco();
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Client non connesso o irraggiungibile");
            }
        }
        else if(client1_gioco_a && client2_gioco_a == false)
        {
            handlers[0].Send(Encoding.UTF8.GetBytes("one_client"));
        }
        else if (client1_gioco_a==false && client2_gioco_a)
        {
            handlers[1].Send(Encoding.UTF8.GetBytes("one_client"));
        }



    }
    static void Invia(Socket source) // metodo "di base" non utilizzato
    {
        string ackMessage = "INVIA";
        byte[] ackBytes = Encoding.UTF8.GetBytes(ackMessage);
        try
        {
            if (source == handlers[0])
            {
                handlers[1].Send(ackBytes);
            }
            else
            {
                handlers[0].Send(ackBytes);
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine("Client non connesso o irraggiungibile");
        }
        
    }

    static void Gioco()  // mescola non funziona
    {
        Mazzo mazzo = new Mazzo();
        mazzo.Mescola();
        
        int i = 52;
        while (i > 10)
        {
            i--;
            Carta carta = mazzo.DistribuisciCarta();
            Console.WriteLine($"Carta distribuita: {carta}");
        }


    }
}
