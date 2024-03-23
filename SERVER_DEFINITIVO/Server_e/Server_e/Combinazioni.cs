using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_e
{
    public class Combinazioni
    {
        public enum Combinazione
        {
            CartaAlta = 1,
            Coppia,
            DoppiaCoppia,
            Tris,
            Scala,
            Colore,
            Full,
            Poker,
            ScalaColore,
            ScalaReale
        }

        public static Combinazione ValutaMano(List<Carta> hand, List<Carta> tavolo) // giocatore, tavolo
        {
            foreach (Carta c in tavolo) // le carte del tavolo
            {
                hand.Add(c);
            }

            if (IsScalaReale(hand)) return Combinazione.ScalaReale;
            if (IsScalaColore(hand)) return Combinazione.ScalaColore;
            if (IsPoker(hand)) return Combinazione.Poker;
            if (IsFull(hand)) return Combinazione.Full;
            if (IsColore(hand)) return Combinazione.Colore;
            if (IsScala(hand)) return Combinazione.Scala;
            if (IsTris(hand)) return Combinazione.Tris;
            if (IsDoppiaCoppia(hand)) return Combinazione.DoppiaCoppia;
            if (IsCoppia(hand)) return Combinazione.Coppia;
            return Combinazione.CartaAlta;
        }

        private static bool IsScalaReale(List<Carta> hand)
        {
            return IsScalaColore(hand) && hand.All(card => card._Valore >= Carta.Valore.Dieci);
        }

        private static bool IsScalaColore(List<Carta> hand)
        {
            return IsColore(hand) && IsScala(hand);
        }

        private static bool IsPoker(List<Carta> hand)  // poker (4 uguali)
        {
            var rankGroups = hand.GroupBy(card => card._Valore);
            return rankGroups.Any(group => group.Count() == 4);
        }

        private static bool IsFull(List<Carta> hand)  // full (tris + coppia)
        {
            var rankGroups = hand.GroupBy(card => card._Valore);
            return rankGroups.Any(group => group.Count() == 3) && rankGroups.Any(group => group.Count() == 2);
        }

        private static bool IsColore(List<Carta> hand)
        {
            return hand.GroupBy(card => card._Seme).Any(group => group.Count() >= 5);
        }

        private static bool IsScala(List<Carta> hand)  // scala
        {
            List<int> list_valori = new List<int>();
            foreach (Carta c in hand)
            {
                list_valori.Add((int)c._Valore);
            }
            list_valori.Sort();

            if (list_valori.Last() == 14 && list_valori.First() == 2)  // se c'è una scala del tipo A2345  porta asso (valore 1) all'inizio
            {
                list_valori.Remove(list_valori.Last());
                list_valori.Insert(0, 1);
            }

            for (int i = 0; i < 3; i++)
            {
                bool scala = true;
                for (int j = i + 1; j < i + 5; j++)
                {
                    if (list_valori[j] != list_valori[j - 1] + 1)
                    {
                        scala = false;
                    }
                }
                if (scala == true) return true;
            }
            return false;
        }

        private static bool IsTris(List<Carta> hand)
        {
            var rankGroups = hand.GroupBy(card => card._Valore);
            return rankGroups.Any(group => group.Count() == 3);
        }

        private static bool IsDoppiaCoppia(List<Carta> hand)
        {
            var rankGroups = hand.GroupBy(card => card._Valore);
            return rankGroups.Count(group => group.Count() == 2) == 2;
        }

        private static bool IsCoppia(List<Carta> hand)
        {
            var rankGroups = hand.GroupBy(card => card._Valore);
            return rankGroups.Any(group => group.Count() == 2);
        }
    }
}
