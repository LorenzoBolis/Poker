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

        private void ricezione() // in attesa dell'altro client e poi riceve carte
        {
            string response = Program.Ricevi();
            string[] parti = response.Split("|");
            if (parti[0] == "game_started")
            {
                button1.Visible = false;
                button2.Visible = false;
                label1.Visible = false;
                this.BackColor = Color.DarkGreen;

                ricezione_gioco();
            }
        }
        private void ricezione_gioco()  // riceve carte 
        {
            stato_di_gioco = "PREFLOP";
            string cards = Program.Ricevi();
            string[] parti = cards.Split("|"); // 0-1 giocatore 2-6  tavolo

            

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
            label4.Text = mio_nome;
            if (mio_nome == "Client1")
            {
                c1_g1.Image = Image.FromFile("../../../mazzo/" + parti[0].ToLower() + ".jpg"); //c1 giocatore
                c2_g1.Image = Image.FromFile("../../../mazzo/" + parti[1].ToLower() + ".jpg"); // c2 giocatore
                c1_g2.Image = Image.FromFile("../../../mazzo/dorso.jpg");  // c1 avversario
                c2_g2.Image = Image.FromFile("../../../mazzo/dorso.jpg");  // c2 avversario
            }
            else if (mio_nome == "Client2")
            {
                c1_g2.Image = Image.FromFile("../../../mazzo/" + parti[0].ToLower() + ".jpg"); //c1 giocatore
                c2_g2.Image = Image.FromFile("../../../mazzo/" + parti[1].ToLower() + ".jpg"); // c2 giocatore
                c1_g1.Image = Image.FromFile("../../../mazzo/dorso.jpg");  // c1 avversario
                c2_g1.Image = Image.FromFile("../../../mazzo/dorso.jpg");  // c2 avversario
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
            if (res == "Connesso al server")
            {
                button2.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            Program.Invia("GAME");
            string response = Program.Ricevi();
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
            else if (parti[0] == "one_client")
            {
                label1.Text = "In attesa dell'altro giocatore";
                //Thread t1 = new Thread(ricezione);
                //t1.Start();
                ricezione();
            }
            
        }
        private void Gioco()  // preflop  -->  flop(3) -->  turn(1)  -->  river(1)
        {
            Program.Invia("CHECK");
            string messaggio = Program.Ricevi();
            if (messaggio == "OTHER_FOLDED")
            {   
                ripristina();
                MessageBox.Show("HAI VINTO (L'ALTRO GIOCATORE HA LASCIATO)");
                return;
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



        }
        private void button3_Click(object sender, EventArgs e) // check
        {
            Thread th = new Thread(Gioco);
            th.Start();
        }

        private void button4_Click(object sender, EventArgs e)  // fold
        {
            Program.Invia("FOLD");
            ripristina();
        }
    }
}
