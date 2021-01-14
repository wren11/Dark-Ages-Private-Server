#region

using System;
using System.Windows.Forms;
using DAClient.ClientFormats;

#endregion

namespace DAClient.Forms
{
    public partial class ClientForm : Form
    {
        private readonly Client _client;

        public ClientForm(Client client)
        {
            _client = client;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _client?.Send(new Assail());
        }

        private void ClientForm_Load(object sender, EventArgs e)
        {
        }
    }
}