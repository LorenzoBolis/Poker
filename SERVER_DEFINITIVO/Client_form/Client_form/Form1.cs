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
        private void ricezione()
        {
            string response = Program.Ricevi();
            if (response == "game_started")
            {
                button1.Visible = false;
                button2.Visible = false;
                label1.Visible = false;
                this.BackColor = Color.DarkGreen;
            }
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
            }
            else if (response == "one_client")
            {
                label1.Text = "In attesa dell'altro giocatore";
                Thread attendi = new Thread(ricezione);
                attendi.Start();
            }
        }
    }
}
