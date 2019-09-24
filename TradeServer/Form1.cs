using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TradeServer
{


    public partial class Form1 : Form
    {
        public Server Server = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Server = new Server(int.Parse(textBox1.Text));

            //// Создаем поток
            //Thread Thread = new Thread(new ParameterizedThreadStart(ServerThread));
            //    //() => 
            //    //{
            //    //    Server = new Server(int.Parse(textBox1.Text));
            //    //}
            //    //));
            //// И запускаем этот поток, передавая ему принятого клиента
            //Thread.Start(null);
        }

        public void ServerThread(Object StateInfo)
        {
            Server = new Server(int.Parse(textBox1.Text));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Server?.Stop();
        }
    }
}
