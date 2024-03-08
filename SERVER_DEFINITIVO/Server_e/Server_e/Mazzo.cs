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
        private List<Carta> carte;

        public Mazzo()
        {
            carte = CreareMazzo();
        }

        private List<Carta> CreareMazzo()
        {
            var mazzo = new List<Carta>();
            foreach (Seme seme in Enum.GetValues(typeof(Seme)))
            {
                foreach (Valore valore in Enum.GetValues(typeof(Valore)))
                {
                    mazzo.Add(new Carta(seme, valore));
                }
            }
            return mazzo;
        }

        public void Mescola() // TODO - mescola non funziona
        {
            Random r1 = new Random();
            Random r2 = new Random();
            List<Carta> newmazzo = CreareMazzo();
            for (int i = 0; i==100; i++) 
            {
                int pos1 = r1.Next(0, 53);
                int pos2 = r1.Next(0, 53);
                Carta c = newmazzo[pos1];
                newmazzo[pos1] = newmazzo[pos2];
                newmazzo[pos2] = c;
            }
            carte = newmazzo;
        }

        public Carta DistribuisciCarta()
        {
            if (carte.Count == 0)
            {
                throw new InvalidOperationException("Il mazzo è vuoto.");
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
