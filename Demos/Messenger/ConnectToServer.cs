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
    public partial class ConnectToServer : Form
    {
        FirstForm Form;
        public ConnectToServer(FirstForm form)
        {
            Form = form;
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {

        }

        private void ConnectToServer_Load(object sender, EventArgs e)
        {

        }
    }
}
