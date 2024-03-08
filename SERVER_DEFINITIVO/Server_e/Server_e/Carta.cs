using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_e
{
    public class Carta
    {
        private Seme seme;
        private Valore valore;
        public enum Seme
        {
            Cuori,
            Quadri,
            Fiori,
            Picche
        }

        public enum Valore
        {
            Due = 2,
            Tre,
            Quattro,
            Cinque,
            Sei,
            Sette,
            Otto,
            Nove,
            Dieci,
            Jack,
            Regina,
            Re,
            Asso
        }
        public Seme _Seme
        {
            get { return seme; }
            set {  seme = value; }
        }
        public Valore _Valore
        {
            get { return valore;}
            set { valore = value; }
        }

        public Carta(Seme seme, Valore valore)
        {
            _Seme = seme;
            _Valore = valore;
        }

        public override string ToString()
        {
            return $"{_Valore}_di_{_Seme}";
        }
    }
}
