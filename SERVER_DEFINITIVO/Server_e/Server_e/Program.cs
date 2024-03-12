using Server_e;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

class Program  // SERVER - 192.168.1.139  (184 mik)
{
    
    private static List<Giocatore> giocatori = new List<Giocatore>();// nome - socket - mano - gioco_avviato
    private static List<Carta> carte_tavolo = new List<Carta>();

    [STAThread]
    static void Main()
    {
        int contatore = 0;
        

        // Initialize socket
        Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.139"), 9000);

        // Bind socket to IP and port
        listener.Bind(ipEndPoint);
        listener.Listen(4);

        Console.WriteLine("Server in ascolto su {0}", ipEndPoint);

        while (contatore <= 1)
        {
            // Accetta connessioni in arrivo
            Socket client = listener.Accept();

            // Invia messaggio di connessione
            string message = "CONNECTED";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            client.Send(messageBytes);

            byte[] buffer = new byte[1024];
            int received = client.Receive(buffer);
            string response = Encoding.UTF8.GetString(buffer, 0, received);

            if (response == "ACK")
            {
                contatore++;
                Giocatore g = new Giocatore("Client"+contatore.ToString(), client);
                giocatori.Add(g);
                Console.WriteLine($"Nuova connessione da {g.Sk.RemoteEndPoint} - {g.Nome}");

                // Crea un thread separato per gestire la connessione
                Thread clientThread = new Thread(() =>
                {
                    HandleClient(g);
                });
                clientThread.Start();
            }
        }
        while (true)
        {
            Socket nooo = listener.Accept();
            nooo.Send(Encoding.UTF8.GetBytes("NOT_CONN")); // invia messaggio di NON connessione
            nooo.Close();
        }
    }

    static void HandleClient(Giocatore g)
    {
        string name = g.Nome;
        Socket client = g.Sk;
        try
        {
            string message = "";
            // Gestisci il flusso di dati con il singolo client
            do
            {
                byte[] received = new byte[1024];
                int receivedBytes = client.Receive(received);
                message = Encoding.UTF8.GetString(received, 0, receivedBytes);
                if (message == "exit")
                {
                    Console.WriteLine($"Chiusura connessione con {name}");
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    break;
                }
                else if (message == "GAME")
                {
                    g.Gioco_avviato = true;
                    
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
        if (giocatori.Count < 2)
        {
            giocatori[0].Sk.Send(Encoding.UTF8.GetBytes("one_client"));
        }
        else
        {
            if (giocatori[0].Gioco_avviato && giocatori[1].Gioco_avviato)
            {
                try
                {
                    giocatori[1].Sk.Send(ackBytes);
                    giocatori[0].Sk.Send(ackBytes);
                
                    //Thread giocooo = new Thread(Gioco);
                    //giocooo.Start();
                    Gioco();
                    //return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Client non connesso o irraggiungibile");
                }
            }
            else if (giocatori[0].Gioco_avviato && giocatori[1].Gioco_avviato == false)
            {
                giocatori[0].Sk.Send(Encoding.UTF8.GetBytes("one_client"));
            }
            else if (giocatori[0].Gioco_avviato ==false && giocatori[1].Gioco_avviato)
            {
                giocatori[1].Sk.Send(Encoding.UTF8.GetBytes("one_client"));
            }
        }
        



    }
    /*static void Invia(Socket source) // metodo "di base" non utilizzato
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
        
    }*/

    static void Gioco()
    {
        Mazzo mazzo = new Mazzo();
        mazzo.Mescola();
        Carta c1 = mazzo.DistribuisciCarta();  // giocatore1
        Carta c2 = mazzo.DistribuisciCarta();  // giocatore1
        Carta c3 = mazzo.DistribuisciCarta();  // giocatore2
        Carta c4 = mazzo.DistribuisciCarta();  // giocatore2
        Carta t1 = mazzo.DistribuisciCarta();  //tavolo
        Carta t2 = mazzo.DistribuisciCarta();  //tavolo
        Carta t3 = mazzo.DistribuisciCarta();  //tavolo
        Carta t4 = mazzo.DistribuisciCarta();  //tavolo
        Carta t5 = mazzo.DistribuisciCarta();  //tavolo
        carte_tavolo.Add(t1);
        carte_tavolo.Add(t2);
        carte_tavolo.Add(t3);
        carte_tavolo.Add(t4);
        carte_tavolo.Add(t5);

        giocatori[0].Sk.Send(Encoding.UTF8.GetBytes(c1.ToString() + "|" + c3.ToString() + "|" + t1.ToString() + "|" + t2.ToString() + "|" + t3.ToString() + "|" + t4.ToString() + "|" + t5.ToString()));
        giocatori[1].Sk.Send(Encoding.UTF8.GetBytes(c2.ToString() + "|" + c4.ToString() + "|" + t1.ToString() + "|" + t2.ToString() + "|" + t3.ToString() + "|" + t4.ToString() + "|" + t5.ToString()));
    }
}
