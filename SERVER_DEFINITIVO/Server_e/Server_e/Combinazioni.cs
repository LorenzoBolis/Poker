using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_e
{
    public class Combinazioni
    {
        public enum HandRank
        {
            HighCard,
            OnePair,
            TwoPair,
            ThreeOfAKind,
            Straight,
            Flush,
            FullHouse,
            FourOfAKind,
            StraightFlush,
            RoyalFlush
        }

        public static HandRank EvaluateHand(List<Carta> hand)
        {
            if (IsRoyalFlush(hand)) return HandRank.RoyalFlush;
            if (IsStraightFlush(hand)) return HandRank.StraightFlush;
            if (IsFourOfAKind(hand)) return HandRank.FourOfAKind;
            if (IsFullHouse(hand)) return HandRank.FullHouse;
            if (IsFlush(hand)) return HandRank.Flush;
            if (IsStraight(hand)) return HandRank.Straight;
            if (IsThreeOfAKind(hand)) return HandRank.ThreeOfAKind;
            if (IsTwoPair(hand)) return HandRank.TwoPair;
            if (IsOnePair(hand)) return HandRank.OnePair;
            return HandRank.HighCard;
        }

        private static bool IsRoyalFlush(List<Carta> hand)
        {
            return IsStraightFlush(hand) && hand.All(card => card._Valore >= Carta.Valore.Dieci);
        }

        private static bool IsStraightFlush(List<Carta> hand)
        {
            return IsFlush(hand) && IsStraight(hand);
        }

        private static bool IsFourOfAKind(List<Carta> hand)
        {
            var rankGroups = hand.GroupBy(card => card._Valore);
            return rankGroups.Any(group => group.Count() == 4);
        }

        private static bool IsFullHouse(List<Carta> hand)
        {
            var rankGroups = hand.GroupBy(card => card._Valore);
            return rankGroups.Any(group => group.Count() == 3) && rankGroups.Any(group => group.Count() == 2);
        }

        private static bool IsFlush(List<Carta> hand)
        {
            return hand.GroupBy(card => card._Valore).Count() == 1;
        }

        private static bool IsStraight(List<Carta> hand)
        {
            var sortedRanks = hand.Select(card => (int)card._Valore).OrderBy(rank => rank).ToList();
            if (sortedRanks.Last() == (int)Carta.Valore.Asso && sortedRanks.First() == (int)Carta.Valore.Due)
            {
                // Handle A-2-3-4-5 as a valid straight (wheel)
                sortedRanks.Remove(sortedRanks.Last());
                sortedRanks.Insert(0, 1);
            }
            for (int i = 1; i < sortedRanks.Count; i++)
            {
                if (sortedRanks[i] != sortedRanks[i - 1] + 1)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsThreeOfAKind(List<Carta> hand)
        {
            var rankGroups = hand.GroupBy(card => card._Valore);
            return rankGroups.Any(group => group.Count() == 3);
        }

        private static bool IsTwoPair(List<Carta> hand)
        {
            var rankGroups = hand.GroupBy(card => card._Valore);
            return rankGroups.Count(group => group.Count() == 2) == 2;
        }

        private static bool IsOnePair(List<Carta> hand)
        {
            var rankGroups = hand.GroupBy(card => card._Valore);
            return rankGroups.Any(group => group.Count() == 2);
        }
    }
}
