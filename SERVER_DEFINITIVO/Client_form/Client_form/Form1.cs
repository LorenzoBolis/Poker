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
        private void ricezione() // in attesa dell'altro client e poi riceve carte
        {
            string response = Program.Ricevi();
            if (response == "game_started")
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
            string cards = Program.Ricevi();
            string[] parti = cards.Split("|");
            string card1 = parti[0];
            string card2 = parti[1];
            label2.Visible = true;
            label3.Visible = true;
            pictureBox1.Visible = true;
            pictureBox2.Visible = true;

            label2.Text = card1;
            label3.Text = card2;

            pictureBox1.Image = Image.FromFile("../../../mazzo/" + card1.ToLower() + ".jpg");
            pictureBox2.Image = Image.FromFile("../../../mazzo/" + card2.ToLower() + ".jpg");
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
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
            if (response == "game_started")
            {
                button1.Visible = false;
                button2.Visible = false;
                label1.Visible = false;
                this.BackColor = Color.DarkGreen;
                ricezione_gioco();
            }
            else if (response == "one_client")
            {
                label1.Text = "In attesa dell'altro giocatore";
                //Thread t1 = new Thread(ricezione);
                //t1.Start();
                ricezione();
            }
        }
    }
}
