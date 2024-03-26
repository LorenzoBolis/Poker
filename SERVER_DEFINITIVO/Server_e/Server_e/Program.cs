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
    private static string stato_di_gioco = ""; // preflop  -->  flop(3) -->  turn(1)  -->  river(1)
    private static int pot;
    private static Giocatore vincitore;

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
                    
                    Invia_Start(name);
                }
                else if (message == "CHECK") 
                {
                    g.Check = true;
                    Giocatore altro = giocatori[0];
                    if (giocatori[0] == g) // determina chi è l'altro giocatore
                    {
                        altro = giocatori[1];
                    }
                    else if (giocatori[1] == g)
                    {
                        altro = giocatori[0];
                    }
                    /*if (altro.Check == false)  // se l'altro non ha ancora fatto la giocata, viene aggiornato
                    {
                        
                    }*/
                    Invia_Giocata_altro(g, "CHECK");

                    Gioco_Check();
                }
                else if (message.Contains("RAISE"))
                {
                    int rilancio = int.Parse(message.Split("|")[1]);
                    g.Check = true;
                    Gioco_Raise(rilancio);
                }
                else if (message == "FOLD")
                {
                    Invia_Giocata_altro(g, "FOLDED");
                    
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
    static void Invia_Start(string nome_client)
    {
        string ackMessage = "game_started"+"|"+nome_client;
        byte[] ackBytes = Encoding.UTF8.GetBytes(ackMessage);
        if (giocatori.Count < 2)
        {
            giocatori[0].Sk.Send(Encoding.UTF8.GetBytes("one_client" + "|" + nome_client));
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
                    Carte_Gioco();
                    //return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Client non connesso o irraggiungibile");
                }
            }
            else if (giocatori[0].Gioco_avviato && giocatori[1].Gioco_avviato == false)
            {
                giocatori[0].Sk.Send(Encoding.UTF8.GetBytes("one_client" + "|" + nome_client));
            }
            else if (giocatori[0].Gioco_avviato ==false && giocatori[1].Gioco_avviato)
            {
                giocatori[1].Sk.Send(Encoding.UTF8.GetBytes("one_client" + "|" + nome_client));
            }
        }
    }
    static void Gioco_Check()
    {
        pot = 200; // TODO somma delle puntate giocatori
        if (stato_di_gioco == "PREFLOP")
        {
            Invia_Flop();
        }
        else if (stato_di_gioco == "FLOP")
        {
            Invia_Turn();
        }
        else if (stato_di_gioco == "TURN")
        {
            Invia_River();
        }
    }

    private static void Gioco_Raise(int raise)
    {
        pot += raise;
        if (stato_di_gioco == "PREFLOP")
        {
            Invia_Flop();
        }
        else if (stato_di_gioco == "FLOP")
        {
            Invia_Turn();
        }
        else if (stato_di_gioco == "TURN")
        {
            Invia_River();
        }
    }

    private static void Fine_Partita()
    {
        //valutazione carte
        var combinazione1 = Combinazioni.ValutaMano(giocatori[0].Mano, carte_tavolo);
        var combinazione2 = Combinazioni.ValutaMano(giocatori[1].Mano, carte_tavolo);
        Console.WriteLine("Giocatore 1  :  " + combinazione1);
        Console.WriteLine("Giocatore 2  :  " + combinazione2);
        
        if ((int)combinazione1 > (int)combinazione2)
        {
            Console.WriteLine("Vince G1");
            giocatori[0].Sk.Send(Encoding.UTF8.GetBytes($"VINTO|{pot}"));
            giocatori[1].Sk.Send(Encoding.UTF8.GetBytes($"PERSO|{pot}"));
        }
        else if ((int)combinazione1 < (int)combinazione2)
        {
            Console.WriteLine("Vince G2");
            giocatori[0].Sk.Send(Encoding.UTF8.GetBytes($"PERSO|{pot}"));
            giocatori[1].Sk.Send(Encoding.UTF8.GetBytes($"VINTO|{pot}"));
        }
        else
        {
            Console.WriteLine("Pareggio");
            giocatori[0].Sk.Send(Encoding.UTF8.GetBytes($"PAREGGIO|{pot}"));
            giocatori[1].Sk.Send(Encoding.UTF8.GetBytes($"PAREGGIO|{pot}"));
        }
    }
private static void Carte_Gioco()
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

        giocatori[0].Sk.Send(Encoding.UTF8.GetBytes(c1.ToString() + "|" + c2.ToString()));  //  + "|" + t1.ToString() + "|" + t2.ToString() + "|" + t3.ToString() + "|" + t4.ToString() + "|" + t5.ToString()
        giocatori[1].Sk.Send(Encoding.UTF8.GetBytes(c3.ToString() + "|" + c4.ToString()));  //  + "|" + t1.ToString() + "|" + t2.ToString() + "|" + t3.ToString() + "|" + t4.ToString() + "|" + t5.ToString()
        stato_di_gioco = "PREFLOP";
    }

    private static void Invia_Giocata_altro(Giocatore g, string giocata) // giocatore che ha fatto la giocata
    {
        if (giocatori[0] == g)
        {
            giocatori[1].Sk.Send(Encoding.UTF8.GetBytes("OTHER_"+giocata));
        }
        else if (giocatori[1] == g)
        {
            giocatori[0].Sk.Send(Encoding.UTF8.GetBytes("OTHER_" + giocata));
        }
    }

    private static void Invia_Flop()
    {
        string ackMessage = "CHECKED" + "|" + carte_tavolo[0].ToString() + "|" + carte_tavolo[1].ToString() + "|" + carte_tavolo[2].ToString();
        byte[] ackBytes = Encoding.UTF8.GetBytes(ackMessage);
        if (giocatori[0].Check && giocatori[1].Check)
        {
            try
            {
                giocatori[1].Sk.Send(ackBytes);
                giocatori[0].Sk.Send(ackBytes);
                stato_di_gioco = "FLOP";
                giocatori[0].Check = false;
                giocatori[1].Check = false;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Client non connesso o irraggiungibile");
            }
        }
    }

    private static void Invia_Turn()
    {
        string ackMessage = "CHECKED" + "|" + carte_tavolo[3].ToString();
        byte[] ackBytes = Encoding.UTF8.GetBytes(ackMessage);
        if (giocatori[0].Check && giocatori[1].Check)
        {
            try
            {
                giocatori[1].Sk.Send(ackBytes);
                giocatori[0].Sk.Send(ackBytes);
                stato_di_gioco = "TURN";
                giocatori[0].Check = false;
                giocatori[1].Check = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Client non connesso o irraggiungibile");
            }
        }
    }

    private static void Invia_River()
    {
        string ackMessage = "CHECKED" + "|" + carte_tavolo[4].ToString();
        byte[] ackBytes = Encoding.UTF8.GetBytes(ackMessage);
        if (giocatori[0].Check && giocatori[1].Check)
        {
            try
            {
                giocatori[1].Sk.Send(ackBytes);
                giocatori[0].Sk.Send(ackBytes);
                stato_di_gioco = "RIVER";
                giocatori[0].Check = false;
                giocatori[1].Check = false;
                Fine_Partita();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Client non connesso o irraggiungibile");
            }
        }
    }

    
}
