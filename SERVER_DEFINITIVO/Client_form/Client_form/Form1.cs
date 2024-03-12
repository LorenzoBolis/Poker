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
            string[] parti = cards.Split("|"); // 0-1 giocatore 2-6  tavolo

            label2.Visible = true;
            label3.Visible = true;
            label2.Text = parti[0];
            label3.Text = parti[1];

            pictureBox1.Visible = true;
            pictureBox2.Visible = true;
            pictureBox3.Visible = true;
            pictureBox4.Visible = true;
            pictureBox5.Visible = true;   // TODO  -  cambiare disposizione carte in base a se è giocatore1 (destra) o giocatore2 (sinistra)
            pictureBox6.Visible = true;
            pictureBox7.Visible = true;
            pictureBox8.Visible = true;
            pictureBox9.Visible = true;

            pictureBox1.Image = Image.FromFile("../../../mazzo/" + parti[0].ToLower() + ".jpg");
            pictureBox2.Image = Image.FromFile("../../../mazzo/" + parti[1].ToLower() + ".jpg");
            pictureBox3.Image = Image.FromFile("../../../mazzo/dorso.jpg");
            pictureBox4.Image = Image.FromFile("../../../mazzo/dorso.jpg");
            pictureBox5.Image = Image.FromFile("../../../mazzo/" + parti[2].ToLower() + ".jpg");
            pictureBox6.Image = Image.FromFile("../../../mazzo/" + parti[3].ToLower() + ".jpg");
            pictureBox7.Image = Image.FromFile("../../../mazzo/" + parti[4].ToLower() + ".jpg");
            pictureBox8.Image = Image.FromFile("../../../mazzo/" + parti[5].ToLower() + ".jpg");
            pictureBox9.Image = Image.FromFile("../../../mazzo/" + parti[6].ToLower() + ".jpg");
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
