using Server_e;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

class Program  // SERVER - 192.168.0.5
{
    
    private static List<Giocatore> giocatori = new List<Giocatore>();// nome - socket - mano - gioco_avviato
    private static List<Carta> carte_tavolo = new List<Carta>();
    private static Stati stato_di_gioco;  // preflop  -->  flop(3) -->  turn(1)  -->  river(1)
    private static int pot;
    private static Giocatore vincitore;
    private static Mazzo mazzo;

    private enum Stati  // possibili stati di gioco
    {
        Preflop,
        Flop,
        Turn,
        River
    }

    [STAThread]
    static void Main()
    {
        int contatore = 0;
        Crea_Mazzo();

        // Initialize socket
        Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.0.5"), 51000);

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
        try
        {
            string messaggio = "";
            // Gestisci il flusso di dati con il singolo client
            do
            {
                messaggio = g.Receive();
                string[] parti = messaggio.Split("|");
                if (messaggio == "exit")
                {
                    Console.WriteLine($"Chiusura connessione con {name}");
                    g.Sk.Shutdown(SocketShutdown.Both);
                    g.Sk.Close();
                    break;
                }
                else if (parti[0] == "GAME")
                {
                    g.Gioco_avviato = true;
                    g.Fiches -= int.Parse(parti[1]);
                    pot += int.Parse(parti[1]);
                    Invia_Start();
                }
                else if (messaggio == "CHECK") 
                {
                    g.Check = true;
                    Invia_Giocata_altro(g, "CHECK");
                    Gioco_Check();
                }
                else if (messaggio.Contains("RAISE"))
                {
                    int rilancio = int.Parse(messaggio.Split("|")[1]);
                    g.Check = true;
                    Gioco_Raise(rilancio);
                }
                else if (messaggio == "FOLD")
                {
                    if (g.Nome == "Client1")
                    {
                        Invia_Giocata_altro(g, "FOLD");
                        giocatori[1].Fiches += pot;
                    }
                    else
                    {
                        Invia_Giocata_altro(giocatori[0], "FOLD");
                        Invia_Giocata_altro(giocatori[1], "FOLD");
                        giocatori[0].Fiches += pot;
                    }
                    giocatori[0].Mano.Clear();
                    giocatori[1].Mano.Clear();
                    carte_tavolo.Clear();
                    giocatori[0].Gioco_avviato = false;
                    giocatori[1].Gioco_avviato = false;
                    pot = 0;
                }
                
                Console.WriteLine($"Dati ricevuti da {name}: {messaggio}");
            }
            while (messaggio != "exit");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Errore nella gestione del client: " + ex.Message);
        }
    }
    static void Invia_Start()
    {
        if (giocatori.Count != 2)
        {
            Console.WriteLine("Giocatori connessi insufficienti");
            return;
        }
        if (giocatori[0].Gioco_avviato && giocatori[1].Gioco_avviato)
        {
            try
            {
                giocatori[0].Send($"game_started|Client1|{giocatori[0].Fiches}");
                giocatori[1].Send($"game_started|Client2|{giocatori[1].Fiches}");
                Carte_Gioco();
                //return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Client non connesso o irraggiungibile");
            }
        }
    }
    static void Gioco_Check()
    {
            // TODO somma delle puntate giocatori
        if (stato_di_gioco == Stati.Preflop)
        {
            Invia_Flop();
        }
        else if (stato_di_gioco == Stati.Flop)
        {
            Invia_Turn();
        }
        else if (stato_di_gioco == Stati.Turn)
        {
            Invia_River();
        }
        else if (stato_di_gioco == Stati.River)
        {
            Fine_Partita();
        }
    }

    private static void Gioco_Raise(int raise)
    {
        pot += raise;
        if (stato_di_gioco == Stati.Preflop)
        {
            Invia_Flop();
        }
        else if (stato_di_gioco == Stati.Flop)
        {
            Invia_Turn();
        }
        else if (stato_di_gioco == Stati.Turn)
        {
            Invia_River();
        }
    }

    private static void Fine_Partita()
    {
        //valutazione carte
        if (giocatori[0].Check && giocatori[1].Check)
        {
            var combinazione1 = Combinazioni.ValutaMano(giocatori[0].Mano, carte_tavolo);
            var combinazione2 = Combinazioni.ValutaMano(giocatori[1].Mano, carte_tavolo);
            Console.WriteLine("Giocatore 1  :  " + combinazione1);
            Console.WriteLine("Giocatore 2  :  " + combinazione2);
            string testo0, testo1;
            if ((int)combinazione1 > (int)combinazione2)
            {
                Console.WriteLine("Vince G1");
                testo0 = $"VINTO|{pot}";
                testo1 = $"PERSO|{pot}";
                giocatori[0].Fiches += pot;
            }
            else if ((int)combinazione1 < (int)combinazione2)
            {
                Console.WriteLine("Vince G2");
                testo0 = $"PERSO|{pot}";
                testo1 = $"VINTO|{pot}";
                giocatori[1].Fiches += pot;
            }
            else
            {
                List<int> list0 = new List<int>() { (int)giocatori[0].Mano[0]._Valore, (int)giocatori[0].Mano[1]._Valore };
                List<int> list1 = new List<int>() { (int)giocatori[1].Mano[0]._Valore, (int)giocatori[1].Mano[1]._Valore };
                if (list0.Max() > list1.Max()) // in caso di pareggio valuta chi ha carta alta
                {
                    Console.WriteLine("Vince G1");
                    testo0 = $"VINTO|{pot}";
                    testo1 = $"PERSO|{pot}";
                    giocatori[0].Fiches += pot;
                }
                else if (list0.Max() < list1.Max())
                {
                    testo0 = $"PERSO|{pot}";
                    testo1 = $"VINTO|{pot}";
                    giocatori[1].Fiches += pot;
                }
                else
                {
                    testo0 = $"PAREGGIO|{pot}";
                    testo1 = $"PAREGGIO|{pot}";
                    giocatori[0].Fiches += pot/2;
                    giocatori[1].Fiches += pot/2;
                }
            }
            string mess0 = "FINE_MANO|" + testo0;
            string mess1 = "FINE_MANO|" + testo1;
            try
            {
                giocatori[0].Send(mess0);
                giocatori[1].Send(mess1);
                stato_di_gioco = Stati.Preflop;
                giocatori[0].Check = false;
                giocatori[1].Check = false;
                giocatori[0].Mano.Clear();
                giocatori[1].Mano.Clear();
                carte_tavolo.Clear();
                giocatori[0].Gioco_avviato = false;
                giocatori[1].Gioco_avviato = false;
                pot = 0;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Client non connesso o irraggiungibile");
            }
        }
    }
    private static void Carte_Gioco()
    {
        if (mazzo.Carte.Count < 9)
        {
            Crea_Mazzo();
        }
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

        giocatori[0].Send(c1.ToString() + "|" + c2.ToString());  //  + "|" + t1.ToString() + "|" + t2.ToString() + "|" + t3.ToString() + "|" + t4.ToString() + "|" + t5.ToString()
        giocatori[1].Send(c3.ToString() + "|" + c4.ToString());  //  + "|" + t1.ToString() + "|" + t2.ToString() + "|" + t3.ToString() + "|" + t4.ToString() + "|" + t5.ToString()
        stato_di_gioco = Stati.Preflop;
    }
    private static void Crea_Mazzo()
    {
        mazzo = new Mazzo();
        mazzo.Mescola();
    }

    private static void Invia_Giocata_altro(Giocatore g, string giocata) // giocatore che ha fatto la giocata
    {  // invia OTHER+giocata all'altro giocatore
        if (giocatori[0] == g)
        {
            giocatori[1].Send("OTHER_" + giocata);
        }
        else if (giocatori[1] == g)
        {
            giocatori[0].Send("OTHER_" + giocata);
        }
    }

    private static void Invia_Flop()
    {
        string messaggio = "CHECKED" + "|" + carte_tavolo[0].ToString() + "|" + carte_tavolo[1].ToString() + "|" + carte_tavolo[2].ToString();
        if (giocatori[0].Check && giocatori[1].Check)
        {
            try
            {
                giocatori[1].Send(messaggio);
                giocatori[0].Send(messaggio);
                stato_di_gioco = Stati.Flop;
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
        string messaggio = "CHECKED" + "|" + carte_tavolo[3].ToString();
        if (giocatori[0].Check && giocatori[1].Check)
        {
            try
            {
                giocatori[1].Send(messaggio);
                giocatori[0].Send(messaggio);
                stato_di_gioco = Stati.Turn;
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
        string messaggio = "CHECKED" + "|" + carte_tavolo[4].ToString();
        if (giocatori[0].Check && giocatori[1].Check)
        {
            try
            {
                giocatori[1].Send(messaggio);
                giocatori[0].Send(messaggio);
                stato_di_gioco = Stati.River;
                giocatori[0].Check = false;
                giocatori[1].Check = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Client non connesso o irraggiungibile");
            }
        }
    }
}