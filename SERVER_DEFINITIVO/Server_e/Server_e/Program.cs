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
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.36.83"), 9000);

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
    
    static int ValutaCombinazioneCarte(List<Carta> mano)
    {
        // Ordina le carte per valore
        // Unisci le carte della mano e del banco in una singola lista
        List<Carta> carteTotali = new List<Carta>();
        carteTotali.AddRange(mano);
        carteTotali.AddRange(carte_tavolo);

        // Ordina le carte per valore
        carteTotali = carteTotali.OrderBy(carta => carta._Valore).ToList();

        // Verifica se ci sono combinazioni di carte
        bool coppia = false;
        bool tris = false;
        bool colore = false;
        bool scala = false;


        // Conta le carte di ciascun seme
        Dictionary<string, int> conteggioSemi = new Dictionary<string, int>();
        foreach (var carta in mano)
        {
            if (!conteggioSemi.ContainsKey(carta._Seme.ToString()))
            {
                conteggioSemi[carta._Seme.ToString()] = 0;
            }
            conteggioSemi[carta._Seme.ToString()]++;
        }

        // Conta le carte di ciascun valore
        Dictionary<string, int> conteggioValori = new Dictionary<string, int>();
        foreach (var carta in mano)
        {
            if (!conteggioValori.ContainsKey(carta._Valore.ToString()))
            {
                conteggioValori[carta._Valore.ToString()] = 0;
            }
            conteggioValori[carta._Valore.ToString()]++;
        }

        // Verifica la presenza di combinazioni di carte
        foreach (var coppiaValore in conteggioValori)
        {
            if (coppiaValore.Value == 2)
            {
                coppia = true;
            }
            else if (coppiaValore.Value == 3)
            {
                tris = true;
            }
        }

        // Verifica se ci sono cinque carte dello stesso seme (colore)
        foreach (var seme in conteggioSemi)
        {
            if (seme.Value >= 5)
            {
                colore = true;
            }
        }

        // Verifica se ci sono cinque carte consecutive (scala)
        for (int i = 0; i < mano.Count - 4; i++)
        {
            if (mano[i + 4]._Valore == mano[i]._Valore + 4)
            {
                scala = true;
                break;
            }
        }

        // Assegna il punteggio in base alla combinazione di carte
        if (scala && colore)
        {
            return 9; // Scala Reale
        }
        else if (tris)
        {
            return 8; // Poker
        }
        else if (coppia && conteggioValori.Count == 3)
        {
            return 7; // Full
        }
        else if (colore)
        {
            return 6; // Colore
        }
        else if (scala)
        {
            return 5; // Scala
        }
        else if (tris)
        {
            return 4; // Tris
        }
        else if (conteggioValori.Count == 4)
        {
            return 3; // Doppia Coppia
        }
        else if (coppia)
        {
            return 2; // Coppia
        }
        else
        {
            return 1; // Carta Alta
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
        giocatori[0].Aggiungicarta(c1);
        giocatori[0].Aggiungicarta(c2);
        Carta c3 = mazzo.DistribuisciCarta();  // giocatore2
        Carta c4 = mazzo.DistribuisciCarta();  // giocatore2
        giocatori[1].Aggiungicarta(c3);
        giocatori[1].Aggiungicarta(c4);
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

        //Console.WriteLine(ValutaCombinazioneCarte(giocatori[0].Mano));
    }
}
