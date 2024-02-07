using SirSqlValetCore.Utils;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

using F = System.Windows.Forms;

namespace SirSqlValetCommands.Forms
{
    public partial class FKeywords : Form
    {
        private bool throughMyShowDialog    = false;
        private bool ok                     = false;

        private bool Ctrl                   = false;     
        private bool DisableCheckedChanged  = false;

        List<F.CheckBox> checkBoxes = new List<F.CheckBox>();

        public FKeywords(IEnumerable<string> inKeywords, IEnumerable<string> selection)
        {
            InitializeComponent();

            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).BeginInit();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();

            foreach (string key in inKeywords)
            {
                F.CheckBox cb = new F.CheckBox();

                cb.AutoSize     = true;
                cb.Name         = $"cb{checkBoxes.Count}";
                cb.TabIndex     = checkBoxes.Count;
                cb.Text         = key;
                cb.Font         = new Font("Impact", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                cb.ForeColor    = Color.White;

                if (selection.Contains(key))
                    cb.Checked = true;

                SetCheckBoxColor(cb);
                cb.CheckedChanged += Cb_CheckedChanged;

                checkBoxes.Add(cb);
                flowLayoutPanel1.Controls.Add(cb);
            }

            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).EndInit();
            splitContainer1.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        private void SetCheckBoxColor(F.CheckBox cb) => cb.ForeColor = cb.Checked ? Color.White : Color.FromArgb(100, 100, 100);

        private void Cb_CheckedChanged(object sender, EventArgs e)
        {
            F.CheckBox cb = (F.CheckBox)sender;
            SetCheckBoxColor(cb);

            if (cb.Checked && Ctrl && !DisableCheckedChanged)
            {
                Ctrl = false;

                DisableCheckedChanged = true;
                checkBoxes.Where(_ => _.Name != cb.Name).ForEach(_ => _.Checked = false);
                DisableCheckedChanged = false;
            }
        }

        public IEnumerable<string> MyShowDialog()
        {
            throughMyShowDialog = true;
            this.ShowDialog();
            throughMyShowDialog = false;
            return ok ? checkBoxes.Where(_ => _.Checked).Select(_ => _.Text) : new List<string>().AsEnumerable();
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
            ok = checkBoxes.Any(_ => _.Checked);
            this.Close();
        }

        private void TraitementEscape() => this.Close();

        private void FKeywords_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                TraitementEnter();

            else if (e.KeyCode == Keys.Escape)
                TraitementEscape();

            else if (e.KeyCode == Keys.Space && e.Control)
            { 
                if (checkBoxes.All(_ => _.Checked) || checkBoxes.All(_ => !_.Checked))
                    checkBoxes.ForEach(_ => _.Checked = !_.Checked);
                else
                    checkBoxes.ForEach(_ => _.Checked = true);
            }

            else if (Keys.ControlKey == e.KeyCode)
                Ctrl = true;
        }

        private void FKeywords_KeyUp(object sender, KeyEventArgs e)
        {
            if ( Keys.ControlKey == e.KeyCode )
                Ctrl = false;
        }

        private void FKeywords_Resize(object sender, EventArgs e)
        {
            splitContainer1.SplitterDistance = 82;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TraitementEnter();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TraitementEscape();
        }

    }
}
