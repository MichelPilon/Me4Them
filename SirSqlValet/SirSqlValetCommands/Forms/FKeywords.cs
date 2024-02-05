using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SirSqlValetCommands.Forms
{
    public partial class FKeywords : Form
    {
        private bool throughMyShowDialog    = false;
        private bool ok                     = false;

        public FKeywords(IEnumerable<string> inKeywords)
        {
            InitializeComponent();
            listBox1.Items.AddRange(inKeywords.ToArray());
        }

        public IEnumerable<string> MyShowDialog(IEnumerable<string> selection)
        {
            for (int i = 0; i < listBox1.Items.Count; i++)
                if (selection.Contains(listBox1.Items[i]))
                    listBox1.SelectedItems.Add(listBox1.Items[i]);

            throughMyShowDialog = true;
            this.ShowDialog();
            throughMyShowDialog = false;
            return ok ? (from string s in listBox1.SelectedItems select s).AsEnumerable() : new List<string>().AsEnumerable();
        }

        private void FKeywords_Shown(object sender, EventArgs e)
        {
            if (!throughMyShowDialog)
            {
                MessageBox.Show("Use MyShowDialog to display this window", "Sir Sql Valet", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                this.Close();
                return;
            }
        }

        private void TraitementEnter()
        {
            ok = listBox1.SelectedItems.Count > 0;
            this.Close();
        }

        private void TraitementEscape() => this.Close();

        private void FKeywords_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                TraitementEnter();
            else if (e.KeyCode == Keys.Escape)
                TraitementEscape();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e) => TraitementEnter();
    }
}
