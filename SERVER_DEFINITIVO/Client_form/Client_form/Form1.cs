using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Client_form
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }
        private string stato_di_gioco = "";
        private string mio_nome = "";
        private void ricezione_gioco()  // riceve carte 
        {
            stato_di_gioco = "PREFLOP";
            string cards = Program.Ricevi();
            string[] parti = cards.Split("|"); // 0-1 giocatore 2-6  tavolo

            pictureBox1.Image = null;

            button3.Visible = true;
            button4.Visible = true;
            button5.Visible = true;
            button6.Visible = true;

            label2.Visible = true;
            label3.Visible = true;
            label2.Text = parti[0];
            label3.Text = parti[1];

            c1_g1.Visible = true;
            c2_g1.Visible = true;
            c1_g2.Visible = true;
            c2_g2.Visible = true;
            pictureBox5.Visible = true;
            pictureBox6.Visible = true;
            pictureBox7.Visible = true;
            pictureBox8.Visible = true;
            pictureBox9.Visible = true;
            pictureBox1.Visible = false;
            label4.Text = mio_nome;
            label5.Visible = true;
            if (mio_nome == "Client1")
            {
                c1_g1.Image = Image.FromFile("../../../mazzo/" + parti[0].ToLower() + ".jpg"); //c1 giocatore
                c2_g1.Image = Image.FromFile("../../../mazzo/" + parti[1].ToLower() + ".jpg"); // c2 giocatore
                c1_g2.Image = Image.FromFile("../../../mazzo/dorso.jpg");  // c1 avversario
                c2_g2.Image = Image.FromFile("../../../mazzo/dorso.jpg");  // c2 avversario
                button3.Enabled = true;
                button4.Enabled = true;
                button5.Enabled = true;
                button6.Enabled = true;
            }
            else if (mio_nome == "Client2")
            {
                c1_g2.Image = Image.FromFile("../../../mazzo/" + parti[0].ToLower() + ".jpg"); //c1 giocatore
                c2_g2.Image = Image.FromFile("../../../mazzo/" + parti[1].ToLower() + ".jpg"); // c2 giocatore
                c1_g1.Image = Image.FromFile("../../../mazzo/dorso.jpg");  // c1 avversario
                c2_g1.Image = Image.FromFile("../../../mazzo/dorso.jpg");  // c2 avversario
                button3.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
                button6.Enabled = false;
                Thread th = new Thread(Gioco);
                th.Start();
            }
            /*pictureBox5.Image = Image.FromFile("../../../mazzo/" + parti[2].ToLower() + ".jpg");
            pictureBox6.Image = Image.FromFile("../../../mazzo/" + parti[3].ToLower() + ".jpg");
            pictureBox7.Image = Image.FromFile("../../../mazzo/" + parti[4].ToLower() + ".jpg");
            pictureBox8.Image = Image.FromFile("../../../mazzo/" + parti[5].ToLower() + ".jpg");
            pictureBox9.Image = Image.FromFile("../../../mazzo/" + parti[6].ToLower() + ".jpg");*/

        }
        private void ripristina()
        {
            stato_di_gioco = "";
            c1_g1.Image = null;
            c2_g1.Image = null;
            c1_g2.Image = null;
            c2_g2.Image = null;
            pictureBox5.Image = null;
            pictureBox6.Image = null;
            pictureBox7.Image = null;
            pictureBox8.Image = null;
            pictureBox9.Image = null;
            pictureBox1.Visible = false;
            label2.Text = "";
            label3.Text = "";
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string res = Program.Connetti();
            label1.Text = res;
            button1.Enabled = false;
            label5.Text = textBox1.Text;
            if (res == "Connesso al server")
            {
                button2.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            New_mano();
        }

        private void New_mano()
        {
            Program.Invia("GAME");
            string response = Program.Ricevi();
            if (response == "OTHER_FOLD") // soluzione a un problema nella fold (quando il client2 fa la fold al turn river)
            {
                response = Program.Ricevi();
            }
            string[] parti = response.Split('|');
            mio_nome = parti[1];
            if (parti[0] == "game_started")
            {
                button1.Visible = false;
                button2.Visible = false;
                label1.Visible = false;
                this.BackColor = Color.DarkGreen;
                ricezione_gioco();
            }
        }
        private void Gioco()  // preflop  -->  flop(3) -->  turn(1)  -->  river(1)
        {
            string messaggio = Program.Ricevi();

            if (messaggio == "OTHER_FOLD")
            {
                ripristina();
                New_mano();
            }
            if (messaggio == "OTHER_CHECK")
            {
                button3.Enabled = true;
                button4.Enabled = true;
                button5.Enabled = true;
                button6.Enabled = true;
                if (mio_nome == "Client2" && stato_di_gioco == "RIVER")
                {

                }
                else
                {
                    messaggio = Program.Ricevi();
                }
            }
            string[] parti = messaggio.Split("|");
            if (stato_di_gioco == "PREFLOP")
            {
                if (parti[0] == "CHECKED")
                {
                    pictureBox5.Image = Image.FromFile("../../../mazzo/" + parti[1].ToLower() + ".jpg");
                    pictureBox6.Image = Image.FromFile("../../../mazzo/" + parti[2].ToLower() + ".jpg");
                    pictureBox7.Image = Image.FromFile("../../../mazzo/" + parti[3].ToLower() + ".jpg");
                    stato_di_gioco = "FLOP";
                }
            }
            else if (stato_di_gioco == "FLOP")
            {
                if (parti[0] == "CHECKED")
                {
                    pictureBox8.Image = Image.FromFile("../../../mazzo/" + parti[1].ToLower() + ".jpg");
                    stato_di_gioco = "TURN";
                }
            }
            else if (stato_di_gioco == "TURN")
            {
                if (parti[0] == "CHECKED")
                {
                    pictureBox9.Image = Image.FromFile("../../../mazzo/" + parti[1].ToLower() + ".jpg");
                    stato_di_gioco = "RIVER";
                }
            }
            else if (stato_di_gioco == "RIVER")
            {
                if (parti[0] == "FINE_MANO")
                {
                    MessageBox.Show(parti[1] + parti[2]); // TODO gestione fiches giocatori
                    ripristina();
                    New_mano();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e) // check
        {
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            Program.Invia("CHECK");
            Thread th = new Thread(Gioco);
            th.Start();
        }

        private void button4_Click(object sender, EventArgs e)  // fold
        {
            Program.Invia("FOLD");
            ripristina();
            New_mano();
        }

        private void button6_Click(object sender, EventArgs e)  // raise
        {
            if (trackBar1.Visible == false)
            {
                // Mostra la TrackBar
                trackBar1.Visible = true;
                trackBar1.Visible = true;
                button6.Text = "Scegli l'importo";
            }
            else
            {
                // Ottieni l'importo della puntata dal valore della TrackBar
                int betAmount = trackBar1.Value;

                // Qui gestisci la puntata e fai le azioni necessarie
                // Ad esempio, puoi passare la puntata al tuo motore di gioco

                // Ripristina la TrackBar per un'altra puntata
                trackBar1.Visible = false;
                trackBar1.Visible = false;
                button6.Text = "RAISE (VUOTO)";
                int rilancio = trackBar1.Value / 100 * 100;
                Program.Invia($"RAISE|{rilancio}");
            }

        }
    }
}
