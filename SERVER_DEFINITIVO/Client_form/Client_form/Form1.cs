using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Security.Policy;
using System.Windows.Forms;

namespace Client_form
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private static Stati stato_di_gioco;
        private string mio_nome_inserito, other_nome_inserito; // nomi inseriti sulla schermata iniziale
        private string mio_nome = "";  // nome utilizzato dal programma
        private int mie_fiches = 1000, other_fiches = 1000, pot = 0;
        private bool in_attesa; // indica se sta aspettando un messaggio dal server
        private int to_call;  // fiches rilanciate dall'altro giocatore
        private int rilancio; // rilancio mio
        private Form2 f2 = null;
        private static List<string> messages_chat = new List<string>();
        private void aggiungi_mess(string s)
        {
            messages_chat.Add(s);
            Invia_messaggi_form2();
        }
        public static void aggiungi_messaggio(string s)
        {
            messages_chat.Add(s);
        }
        private void Invia_messaggi_form2()
        {
            Form2.ricevi_mess_f1(messages_chat);
        }
        private enum Stati  // possibili stati di gioco
        {
            Preflop,
            Flop,
            Turn,
            River
        }
        
        

        private void ricezione_gioco()  // riceve e mostra carte
        {
            stato_di_gioco = Stati.Preflop;
            string cards = Program.Ricevi();
            string[] parti = cards.Split("|"); // 0-1 carte giocatore

            button_chat.Visible = true;
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
            label_fiches.Text = mie_fiches.ToString();
            label5.Visible = true;
            label_other_fiches.Text = other_nome_inserito + "  " + other_fiches;
            if (mio_nome == "Client1")
            {
                c1_g1.Image = Image.FromFile("../../../mazzo/" + parti[0].ToLower() + ".png"); //c1 giocatore
                c2_g1.Image = Image.FromFile("../../../mazzo/" + parti[1].ToLower() + ".png"); // c2 giocatore
                c1_g2.Image = Image.FromFile("../../../mazzo/dorso.jpg");  // c1 avversario
                c2_g2.Image = Image.FromFile("../../../mazzo/dorso.jpg");  // c2 avversario
                check_button.Enabled = true;
                fold_button.Enabled = true;
                raise_button.Enabled = true;
                label_other_fiches.Location = new Point(245, 622);
                aggiornabuttons();
            }
            else if (mio_nome == "Client2")
            {
                c1_g2.Image = Image.FromFile("../../../mazzo/" + parti[0].ToLower() + ".png"); //c1 giocatore
                c2_g2.Image = Image.FromFile("../../../mazzo/" + parti[1].ToLower() + ".png"); // c2 giocatore
                c1_g1.Image = Image.FromFile("../../../mazzo/dorso.jpg");  // c1 avversario
                c2_g1.Image = Image.FromFile("../../../mazzo/dorso.jpg");  // c2 avversario
                check_button.Enabled = false;
                fold_button.Enabled = false;
                raise_button.Enabled = false;
                label_other_fiches.Location = new Point(868, 622);
                aggiornabuttons();
                Thread th = new Thread(Gioco);
                th.Start();
            }

        }
        private void ripristina()  // pulizia schermata
        {
            label2.Text = "";
            label3.Text = "";
            label_fiches.Text = "";
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
        }

        private void button1_Click(object sender, EventArgs e)  // button connetti a server
        {
            if (textBox2.Text.Length == 0)
            {
                MessageBox.Show("Inserisci prima l'ip del server", "Attenzione", MessageBoxButtons.OK);
                button1.Enabled = false;
                return;
            }
            string res = Program.Connetti(textBox2.Text);
            label1.Text = res;
            button1.Enabled = false;
            if (res == "Connesso al server")
            {
                button2.Enabled = true;
                textBox2.Enabled = false;
            }

            string i = Program.ConnettiCHAT(textBox2.Text);
            

            if (i=="CHAT connessa")
            {
                Thread recieve = new Thread(ricevi_Chat);
                recieve.Start();
                
            }
            
            
        }
        private void ricevi_Chat()  // prende i messaggi chat
        {
            while (true)
            {
                string messaggio = Program.RiceviCHAT();
                string[] parti = messaggio.Split("|");
                aggiungi_mess(parti[1] + " : " + parti[2]);

            }
        }

    

        private void button2_Click(object sender, EventArgs e) // button avvia gioco
        {
            if (button2.Text == "Avvia Gioco")
            {
                textBox1.Visible = false;
                label7.Visible = false;
                button2.Enabled = false;
                mio_nome_inserito = textBox1.Text;
                label5.Text = mio_nome_inserito;

                f2 = new Form2(mio_nome_inserito, messages_chat);
                New_mano();
            }
            else
            {
                this.Close();
            }
        }

        

        private async Task New_mano()
        {

            Program.Invia($"GAME|100|{mio_nome_inserito}");
            string response = await Program.RiceviAsync();
            if (response == "OTHER_FOLD") // soluzione a un problema nella fold (quando il client2 fa la fold al turn river)
            {
                response = Program.Ricevi();
            }
            string[] parti = response.Split('|');
            mio_nome = parti[1];
            mie_fiches = int.Parse(parti[2]);
            other_fiches = int.Parse(parti[3]);
            other_nome_inserito = parti[4];
            label_other_fiches.Text = other_nome_inserito + "  " + other_fiches;
            pot = 200;
            label_pot.Text = "$ " + pot;
            button_combinazioni.Visible = true;
            if (other_fiches < 0) // altro giocatore ha finito le fiches
            {
                Togli_tutto();
                label7.Text = $"Hai Vinto - {other_nome_inserito} ha 0 $\nOra hai 2000 $";
                return;
            }
            if (mie_fiches < 0)
            {
                Togli_tutto();
                label7.Text = "Hai perso - Ora hai 0 $";
                return;
            }
            if (parti[0] == "game_started")  // quando entrambi richiedono una nuova partita
            {
                textBox2.Visible = false;
                button1.Visible = false;
                button2.Visible = false;
                button2.Enabled = true;
                label1.Visible = false;
                label_titolo.Visible = false;
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
                pot += to_call;
                label_pot.Text = "$ " + pot;
                label_other_fiches.Text = other_nome_inserito + "  " + other_fiches;
                call_button.Enabled = true;
                fold_button.Enabled = true;
                call_button.Text = "CALL  $ " + to_call.ToString();
                aggiornabuttons();
            }
            if (messaggio == "OTHER_CALL")
            {
                other_fiches -= rilancio;
                pot += rilancio;
                label_pot.Text = "$ " + pot;
                label_other_fiches.Text = other_nome_inserito + "  " + other_fiches;
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
                    pictureBox5.Image = Image.FromFile("../../../mazzo/" + parti[1].ToLower() + ".png");
                    pictureBox6.Image = Image.FromFile("../../../mazzo/" + parti[2].ToLower() + ".png");
                    pictureBox7.Image = Image.FromFile("../../../mazzo/" + parti[3].ToLower() + ".png");
                    pictureBox5.BringToFront();
                    pictureBox6.BringToFront();
                    pictureBox7.BringToFront();
                    stato_di_gioco = Stati.Flop;
                }
                else if (stato_di_gioco == Stati.Flop)
                {
                    pictureBox8.Image = Image.FromFile("../../../mazzo/" + parti[1].ToLower() + ".png");
                    pictureBox8.BringToFront();
                    stato_di_gioco = Stati.Turn;
                }
                else if (stato_di_gioco == Stati.Turn)
                {
                    pictureBox9.Image = Image.FromFile("../../../mazzo/" + parti[1].ToLower() + ".png");
                    pictureBox9.BringToFront();
                    stato_di_gioco = Stati.River;
                }
            }
            if (parti[0] == "FINE_MANO" && stato_di_gioco == Stati.River)
            {
                string mia_combinazione = parti[3];
                string other_combinaizone = parti[4];
                string carta1_avv = parti[5];
                string carta2_avv = parti[6];
                    //c1_g1.Image = Image.FromFile("../../../mazzo/" + parti[0].ToLower() + ".png"); //c1 giocatore

                if (mio_nome == "Client1")
                {
                    c1_g2.Image = Image.FromFile("../../../mazzo/" + carta1_avv.ToLower() + ".png");  // c1 avversario
                    c2_g2.Image = Image.FromFile("../../../mazzo/" + carta2_avv.ToLower() + ".png");  // c2 avversario

                }
                else if (mio_nome == "Client2")
                {
                    c1_g1.Image = Image.FromFile("../../../mazzo/" + carta1_avv.ToLower() + ".png");  // c1 avversario
                    c2_g1.Image = Image.FromFile("../../../mazzo/" + carta2_avv.ToLower() + ".png");  // c2 avversario
                }

                if (mio_nome == "Client1")
                {
                    label2.Text = other_combinaizone;
                    label3.Text = mia_combinazione;
                }
                else if (mio_nome == "Client2")
                {
                    label2.Text = mia_combinazione;
                    label3.Text = other_combinaizone;
                }
                Thread.Sleep(1000);
                if (parti[1] == "VINTO")
                {
                    mie_fiches += int.Parse(parti[2]);
                    label_pot.Text = "Hai Vinto";
                }
                else if (parti[1] == "PERSO")
                {
                    label_pot.Text = "Hai Perso";
                }
                else if (parti[1] == "PAREGGIO")
                {
                    label_pot.Text = "Pareggio";
                }
                Thread.Sleep(3000);
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
                if (mie_fiches < 100 || other_fiches < 100)
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
                pot += rilancio;
                label_pot.Text = "$ " + pot;
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
            pot += to_call;
            label_pot.Text = "$ " + pot;
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
                    if (button.Name == "raise_button" && (mie_fiches < 100 || other_fiches < 100)) button.Enabled = false;
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
            pictureBox_comb.Visible = false;
        }

        private void controlla_trackbar()
        {
            trackBar1.Visible = false;
            if (mie_fiches >= 100 && other_fiches >= 100) trackBar1.Value = 100;
            raise_button.Text = "RAISE";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            back_table.Location = new Point(-63, -70);
            label5.Font = new Font("Arial", 12);
            label_fiches.Font = new Font("Arial", 13);
            label_pot.Font = new Font("Arial", 13);
            label_pot.BackColor = Color.Green;
            label_other_fiches.Font = new Font("Arial", 11);
            textBox2.Text = Program.Trova_ip();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = true;
        }

        private void button_combinazioni_Click(object sender, EventArgs e)
        {
            if (pictureBox_comb.Visible == false)
            {
                pictureBox_comb.Visible = true;
                pictureBox_comb.BringToFront();
            }
            else pictureBox_comb.Visible = false;
        }

        private void Togli_tutto()  // toglie tutti i controlli dalla form per la fine del gioco
        {
            ripristina();
            button_combinazioni.Visible = false;
            label_other_fiches.Visible = false;
            label_pot.Visible = false;
            fold_button.Visible = false;
            check_button.Visible = false;
            call_button.Visible = false;
            raise_button.Visible = false;
            button2.Text = "Chiudi";
            button2.Visible = true;
            button2.Enabled = true;
            label7.Visible = true;
            label7.Font = new Font(label7.Font, FontStyle.Bold);
            label7.Location = new Point(480, 200);
            label_fiches.Visible = false;
        }
        private void back_table_Click(object sender, EventArgs e)
        {
            pictureBox_comb.Visible = false;
        }

        private void button_chat_Click(object sender, EventArgs e)
        {
            f2 = new Form2(mio_nome_inserito, messages_chat);
            f2.Show();
        }
    }
}
