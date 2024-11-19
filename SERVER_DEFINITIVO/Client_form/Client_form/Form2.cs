using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client_form
{
    public partial class Form2 : Form
    {

        private string nome;
        private static List<string> messaggi;
        public Form2(string name, List<string> messages)
        {
            InitializeComponent();
            nome = name;
            messaggi = messages;
            listBox1.DrawMode = DrawMode.OwnerDrawFixed;
            listBox1.ItemHeight = 40; // Adjust as needed
            listBox1.DrawItem += new DrawItemEventHandler(ListBox1_DrawItem);
        }
        private void ListBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Check if the index is valid
            if (e.Index < 0 || e.Index >= listBox1.Items.Count) return;

            Graphics g = e.Graphics;
            string message = listBox1.Items[e.Index].ToString();

            Color sentColor = Color.FromArgb(175, 255, 255); 
            Color receivedColor = Color.FromArgb(215, 240, 240);

            // Check if the message is sent or received
            bool isSent = message.StartsWith("TU :");
            Color bubbleColor = isSent ? sentColor : receivedColor;

           
            int padding = 10;
            int maxBubbleWidth = e.Bounds.Width - 70; 
            SizeF textSize = e.Graphics.MeasureString(message, e.Font);
            int bubbleWidth = (int)Math.Min(maxBubbleWidth, textSize.Width + 2 * padding);

            Rectangle bubbleRect;
            Rectangle shadowRect;

            if (isSent)
            {
                // Align sent messages to the right
                bubbleRect = new Rectangle(e.Bounds.Right - bubbleWidth - padding, e.Bounds.Y + 5, bubbleWidth, e.Bounds.Height - 10);
                shadowRect = new Rectangle(bubbleRect.X + 3, bubbleRect.Y + 3, bubbleRect.Width, bubbleRect.Height);
            }
            else
            {
                // Align received messages to the left
                bubbleRect = new Rectangle(e.Bounds.X + padding, e.Bounds.Y + 5, bubbleWidth, e.Bounds.Height - 10);
                shadowRect = new Rectangle(bubbleRect.X + 3, bubbleRect.Y + 3, bubbleRect.Width, bubbleRect.Height);
            }

            using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(50, Color.Gray)))
            {
                g.FillRoundedRectangle(shadowBrush, shadowRect, 15);
            }
            using (SolidBrush bubbleBrush = new SolidBrush(bubbleColor))
            {
                g.FillRoundedRectangle(bubbleBrush, bubbleRect, 15);
            }
            TextRenderer.DrawText(g, message, listBox1.Font, bubbleRect, Color.Black,
                                  TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            e.DrawFocusRectangle();
        }
        private void Form2_Load(object sender, EventArgs e)
        {
            //Thread recieve = new Thread(aggiorna);
            //recieve.Start();
            aggiorna();
        }
        private static void aggiorna()
        {
            listBox1.Items.Clear();
            foreach (string s in messaggi)
            {
                listBox1.Items.Add(s);
            }
        }
        public static void ricevi_mess_f1(List<string> m)
        {
            messaggi = m;
            aggiorna();
            listBox1.TopIndex = listBox1.Items.Count - 1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                string messaggio = "TU : " + textBox1.Text;
                Program.InviaCHAT($"*CHAT*|{nome}|{textBox1.Text}");
                listBox1.Items.Add(messaggio);

                textBox1.Text = "";
                Form1.aggiungi_messaggio(messaggio);
            }

        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(button1, EventArgs.Empty);
            }
        }
    }
    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle rect, int radius)
        {
            using (GraphicsPath path = RoundedRect(rect, radius))
            {
                g.FillPath(brush, path);
            }
        }

        private static GraphicsPath RoundedRect(Rectangle rect, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(rect.Location, size);
            GraphicsPath path = new GraphicsPath();

            // Top left arc
            path.AddArc(arc, 180, 90);

            // Top right arc
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Bottom right arc
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Bottom left arc
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}
