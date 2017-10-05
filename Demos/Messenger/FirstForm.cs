using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Messenger
{
    public partial class FirstForm : Form
    {
        public FirstForm()
        {
            InitializeComponent();
        }

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            StartServer startServer = new StartServer(this);
            startServer.Show();
        }

        private void btnConnectToServer_Click(object sender, EventArgs e)
        {
            ConnectToServer connect = new ConnectToServer(this);
            connect.Show();
        }

        private void FirstForm_Load(object sender, EventArgs e)
        {

        }
    }
}
