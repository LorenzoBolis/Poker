using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Security.Policy;

namespace Client_form
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }
        private static Stati stato_di_gioco;
        private string mio_nome = "";
        private int mie_fiches = 1000, other_fiches = 1000;
        private bool in_attesa;
        private int to_call;  // fiches rilanciate dall'altro giocatore
        private int rilancio; // rilancio mio
        private enum Stati  // possibili stati di gioco
        {
            Preflop,
            Flop,
            Turn,
            River
        }
        private void ricezione_gioco()  // riceve carte 
        {
            stato_di_gioco = Stati.Preflop;
            string cards = Program.Ricevi();
            string[] parti = cards.Split("|"); // 0-1 carte giocatore

            pictureBox1.Image = null;
            
            check_button.Visible = true;
            fold_button.Visible = true;
            call_button.Visible = true;
            raise_button.Visible = true;

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
            label_fiches.Text = mie_fiches.ToString();
            label5.Visible = true;
            if (mio_nome == "Client1")
            {
                c1_g1.Image = Image.FromFile("../../../mazzo/" + parti[0].ToLower() + ".jpg"); //c1 giocatore
                c2_g1.Image = Image.FromFile("../../../mazzo/" + parti[1].ToLower() + ".jpg"); // c2 giocatore
                c1_g2.Image = Image.FromFile("../../../mazzo/dorso.jpg");  // c1 avversario
                c2_g2.Image = Image.FromFile("../../../mazzo/dorso.jpg");  // c2 avversario
                check_button.Enabled = true;
                fold_button.Enabled = true;
                raise_button.Enabled = true;
                aggiornabuttons();
            }
            else if (mio_nome == "Client2")
            {
                c1_g2.Image = Image.FromFile("../../../mazzo/" + parti[0].ToLower() + ".jpg"); //c1 giocatore
                c2_g2.Image = Image.FromFile("../../../mazzo/" + parti[1].ToLower() + ".jpg"); // c2 giocatore
                c1_g1.Image = Image.FromFile("../../../mazzo/dorso.jpg");  // c1 avversario
                c2_g1.Image = Image.FromFile("../../../mazzo/dorso.jpg");  // c2 avversario
                check_button.Enabled = false;
                fold_button.Enabled = false;
                raise_button.Enabled = false;
                aggiornabuttons();
                Thread th = new Thread(Gioco);
                th.Start();
            }

        }
        private void ripristina()
        {
            stato_di_gioco = Stati.Preflop;
            call_button.Enabled = false;
            aggiornabuttons();
            c1_g1.Visible = false;
            c2_g1.Visible = false;
            c1_g2.Visible = false;
            c2_g2.Visible = false;
            c1_g1.Image = null;
            c2_g1.Image = null;
            c1_g2.Image = null;
            c2_g2.Image = null;
            pictureBox5.SendToBack();
            pictureBox6.SendToBack();
            pictureBox7.SendToBack();
            pictureBox8.SendToBack();
            pictureBox9.SendToBack();
            pictureBox5.Image = null;
            pictureBox6.Image = null;
            pictureBox7.Image = null;
            pictureBox8.Image = null;
            pictureBox9.Image = null;
            pictureBox1.Visible = false;
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
            if (button2.Text=="Avvia Gioco")
            {
                textBox1.Visible = false;
                label7.Visible = false;
                button2.Enabled = false;
                New_mano();
            }
            else
            {
                this.Close();
            }
        }

        private void New_mano()
        {
            Program.Invia("GAME|100");
            string response = Program.Ricevi();
            if (response == "OTHER_FOLD") // soluzione a un problema nella fold (quando il client2 fa la fold al turn river)
            {
                response = Program.Ricevi();
            }
            string[] parti = response.Split('|');
            mio_nome = parti[1];
            mie_fiches = int.Parse(parti[2]);
            other_fiches = int.Parse(parti[3]);
            if (other_fiches < 0) // altro giocatore ha finito le fiches
            {
                ripristina();
                fold_button.Visible = false;
                check_button.Visible = false;
                call_button.Visible = false;
                raise_button.Visible = false;
                button2.Text = "Chiudi";
                button2.Visible = true;
                button2.Enabled = true;
                label7.Visible = true;
                label7.Font = new Font(label7.Font, FontStyle.Bold);
                label7.Text = "You WIN - other player has 0 $\nYou now have 2000 $";
                label7.Location = new Point(480, 200);
                label_fiches.Visible = false;
                return;
            }
            if (mie_fiches < 0)
            {
                ripristina();
                label7.Visible = true;
                label7.Font = new Font(label7.Font, FontStyle.Bold);
                label7.Text = "You LOST - you have 0 $";
                label7.Location = new Point(480, 200);
                label_fiches.Visible = false;
                fold_button.Visible = false;
                check_button.Visible = false;
                call_button.Visible = false;
                raise_button.Visible = false;
                button2.Text = "Chiudi";
                button2.Visible = true;
                button2.BackColor = Color.Transparent;
                return;
            }
            if (parti[0] == "game_started")
            {
                button1.Visible = false;
                button2.Visible = false;
                button2.Enabled = true;
                label1.Visible = false;
                this.BackColor = Color.DarkGreen;
                back_table.Visible = true;
                ricezione_gioco();
            }
        }
        private void Gioco()  // preflop  -->  flop(3) -->  turn(1)  -->  river(1)
        {
            string messaggio = Program.Ricevi();
            if (messaggio == "OTHER_CHECK")
            {
                if (stato_di_gioco != Stati.River)
                {
                    check_button.Enabled = true;
                    fold_button.Enabled = true;
                    raise_button.Enabled = true;
                    aggiornabuttons();
                }
                if (mio_nome == "Client2" && stato_di_gioco == Stati.River)
                {
                    check_button.Enabled = true;
                    fold_button.Enabled = true;
                    raise_button.Enabled = true;
                    aggiornabuttons();
                }
                else
                {
                    in_attesa = true;
                    messaggio = Program.Ricevi();
                    in_attesa = false;
                }
            }
            if (messaggio == "OTHER_FOLD")
            {
                ripristina();
                New_mano();
            }
            if (messaggio.Contains("OTHER_RAISE"))
            {
                to_call = int.Parse(messaggio.Split("|")[1]);
                other_fiches -= to_call;
                call_button.Enabled = true;
                fold_button.Enabled = true;
                call_button.Text = "CALL  $ " + to_call.ToString();
                aggiornabuttons();
            }
            if (messaggio == "OTHER_CALL")
            {
                other_fiches -= rilancio;
                if (mio_nome == "Client2")
                {
                    check_button.Enabled = false;
                    fold_button.Enabled = false;
                    call_button.Enabled = false;
                    raise_button.Enabled = false;
                    aggiornabuttons();
                }
                else
                {
                    check_button.Enabled = true;
                    fold_button.Enabled = true;
                    raise_button.Enabled = true;
                    call_button.Enabled = false;
                    aggiornabuttons();
                    messaggio = Program.Ricevi();
                }
            }

            string[] parti = messaggio.Split("|");
            if (parti[0] == "CHECKED")
            {
                if (stato_di_gioco == Stati.Preflop)
                {
                    pictureBox5.Image = Image.FromFile("../../../mazzo/" + parti[1].ToLower() + ".jpg");
                    pictureBox6.Image = Image.FromFile("../../../mazzo/" + parti[2].ToLower() + ".jpg");
                    pictureBox7.Image = Image.FromFile("../../../mazzo/" + parti[3].ToLower() + ".jpg");
                    pictureBox5.BringToFront();
                    pictureBox6.BringToFront();
                    pictureBox7.BringToFront();
                    stato_di_gioco = Stati.Flop;
                }
                else if (stato_di_gioco == Stati.Flop)
                {
                    pictureBox8.Image = Image.FromFile("../../../mazzo/" + parti[1].ToLower() + ".jpg");
                    pictureBox8.BringToFront();
                    stato_di_gioco = Stati.Turn;
                }
                else if (stato_di_gioco == Stati.Turn)
                {
                    pictureBox9.Image = Image.FromFile("../../../mazzo/" + parti[1].ToLower() + ".jpg");
                    pictureBox9.BringToFront();
                    stato_di_gioco = Stati.River;
                }
            }
            if (parti[0] == "FINE_MANO" && stato_di_gioco==Stati.River)
            {
                if (parti[1] == "VINTO") mie_fiches += int.Parse(parti[2]);
                MessageBox.Show(parti[1] + parti[2]);
                ripristina();
                New_mano();
            }
        }

        private void button3_Click(object sender, EventArgs e) // check
        {
            controlla_trackbar();
            check_button.Enabled = false;
            fold_button.Enabled = false;
            call_button.Enabled = false;
            raise_button.Enabled = false;
            aggiornabuttons();
            Program.Invia("CHECK");
            Thread th = new Thread(Gioco);
            th.Start();
        }

        private void button4_Click(object sender, EventArgs e)  // fold
        {
            controlla_trackbar();
            Program.Invia("FOLD");
            if (in_attesa == false)
            {
                ripristina();
                New_mano();
            }
        }

        private void button6_Click(object sender, EventArgs e)  // raise
        {
            if (trackBar1.Visible == false)
            {
                // Mostra la TrackBar
                trackBar1.Visible = true;
                if (mie_fiches < other_fiches) trackBar1.Maximum = mie_fiches;
                else trackBar1.Maximum = other_fiches;
                if (mie_fiches<100 || other_fiches < 100)
                {
                    trackBar1.Visible = false;
                    raise_button.Enabled = false;
                    aggiornabuttons();
                    return;
                }
                else
                {
                    trackBar1.Minimum = 100;
                }
                trackBar1.Value = 100;
                trackBar1.BringToFront();
                raise_button.Text = "Select amount $";
            }
            else
            {
                // Ottieni l'importo della puntata dal valore della TrackBar
                rilancio = trackBar1.Value / 100 * 100;
                mie_fiches -= rilancio;
                label_fiches.Text = mie_fiches.ToString();

                // Ripristina la TrackBar per un'altra puntata
                trackBar1.Visible = false;
                trackBar1.Value = 100;
                raise_button.Text = "RAISE";

                Program.Invia($"RAISE|{rilancio}");
                check_button.Enabled = false;
                fold_button.Enabled = false;
                call_button.Enabled = false;
                raise_button.Enabled = false;
                aggiornabuttons();
                if (in_attesa == false)
                {
                    Thread th = new Thread(Gioco);
                    th.Start();
                }
                if (mio_nome == "Client2")
                {
                    Thread aa = new Thread(Gioco);
                    aa.Start();
                    Thread altro = new Thread(Gioco);
                    altro.Start();
                }
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            raise_button.Text = "RAISE  $ " + (trackBar1.Value / 100 * 100).ToString();
        }

        private void call_button_Click(object sender, EventArgs e) // call
        {
            controlla_trackbar();
            call_button.Text = "CALL";
            Program.Invia("CALL|" + to_call);
            mie_fiches -= to_call;
            label_fiches.Text = mie_fiches.ToString();
            if (mio_nome == "Client1")
            {
                check_button.Enabled = true;
                fold_button.Enabled = true;
                raise_button.Enabled = true;
                call_button.Enabled = false;
                aggiornabuttons();
            }
            else
            {
                check_button.Enabled = false;
                fold_button.Enabled = false;
                call_button.Enabled = false;
                raise_button.Enabled = false;
                aggiornabuttons();
                Gioco();
            }
            Thread th = new Thread(Gioco);
            th.Start();
        }

        private void aggiornabuttons()
        {
            foreach (Control control in Controls)
            {
                if (control is Button button)
                {
                    if (button.Name == "raise_button" && mie_fiches < 100) button.Enabled = false;
                    if (button.Enabled)
                    {
                        button.Font = new Font(button.Font, FontStyle.Bold);
                    }
                    else
                    {
                        button.Font = new Font(button.Font, FontStyle.Regular);
                    }
                }
            }
        }

        private void controlla_trackbar()
        {
            trackBar1.Visible = false;
            trackBar1.Value = 100;
            raise_button.Text = "RAISE";
        }
    }
}
