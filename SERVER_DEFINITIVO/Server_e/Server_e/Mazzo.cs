using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Server_e.Carta;

namespace Server_e
{
    public class Mazzo
    {
        private List<Carta> carte = new List<Carta>();
        public List<Carta> Carte
        {
            get { return carte; }
        }
        public Mazzo()
        {
            foreach (Seme seme in Enum.GetValues(typeof(Seme)))
            {
                foreach (Valore valore in Enum.GetValues(typeof(Valore)))
                {
                    carte.Add(new Carta(seme, valore));
                }
            }
        }

        public void Mescola()
        {
            Random r1 = new Random();
            Random r2 = new Random();
            for(int i = 0; i<100; i++)
            {
                int pos1 = r1.Next(0, 52);
                int pos2 = r2.Next(0, 52);
                Carta c = carte[pos1];
                carte[pos1] = carte[pos2];
                carte[pos2] = c;
            }
        }

        public Carta DistribuisciCarta()
        {
            if (carte.Count == 0)
            {
                throw new Exception("Il mazzo è vuoto.");
            }

            Carta cartaDistribuita = carte[0];
            carte.RemoveAt(0);
            return cartaDistribuita;
        }

        public override string ToString()
        {
            return $"Numero di carte nel mazzo: {carte.Count}";
        }
    }
}
