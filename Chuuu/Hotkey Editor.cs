using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chuuu {
    public partial class Hotkey_Editor : Form {
        public Hotkey_Editor() {
            InitializeComponent();
            OK.DialogResult = DialogResult.OK;
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e) {

        }

        private void label2_Click(object sender, EventArgs e) {

        }

        private void OK_Click(object sender, EventArgs e) {

        }
    }
}
