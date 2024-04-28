using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Server_e
{
    public class Giocatore
    {
        private string nome;
        private string nome_inserito;
        private List<Carta> mano = new List<Carta>();
        private Socket sk;
        private int fiches;
        private bool gioco_avviato;
        private bool check;

        public bool Check
        {
            get { return check; }
            set { check = value; }
        }
        public string Nome
        {
            get { return nome; }
            set { nome = value; }
        }
        public string Nome_inserito
        {
            get { return nome_inserito; }
            set { nome_inserito = value; }
        }

        public List<Carta> Mano
        {
            get { return mano; }
            set { mano = value; }
        }

        public Socket Sk
        {
            get { return sk; }
            set { sk = value; }
        }

        public int Fiches
        {
            get { return fiches; }
            set { fiches = value; }
        }

        public bool Gioco_avviato
        {
            get { return gioco_avviato; }
            set { gioco_avviato = value; }
        }

        public Giocatore(string n, Socket sk)
        {
            Nome = n;
            Sk = sk;
            Fiches = 1000;
            Gioco_avviato = false;
        }
        public void Aggiungicarta(Carta carta)
        {
            mano.Add(carta);
        }

        public void Send(string messaggio)
        {
            Sk.Send(Encoding.UTF8.GetBytes(messaggio));
        }

        public string Receive()
        {
            byte[] received = new byte[1024];
            int receivedBytes = Sk.Receive(received);
            return Encoding.UTF8.GetString(received, 0, receivedBytes);
        }
    }
}
