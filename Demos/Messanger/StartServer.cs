using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Messanger
{
    public partial class StartServer : Form
    {
        FirstForm Form;
        public StartServer(FirstForm form)
        {
            Form = form;
            InitializeComponent();
        }

        private void btnStartServer_Click(object sender, EventArgs e)
        {

        }
    }
}
