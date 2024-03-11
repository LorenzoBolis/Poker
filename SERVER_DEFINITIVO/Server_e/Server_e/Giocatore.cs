using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server_e
{
    public class Giocatore
    {
        private string nome;
        private List<Carta> mano;
        private Socket sk;

        public string Nome
        {
            get { return nome; }
            set { nome = value; }
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

        public Giocatore(string n, Socket sk)
        {
            Nome = n;
            Sk = sk;
        }
    }
}
