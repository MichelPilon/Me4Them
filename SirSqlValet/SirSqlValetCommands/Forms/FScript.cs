using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

using System.Windows.Forms;

using SirSqlValetCommands;
using SirSqlValetCommands.Data;

using static SirSqlValetCommands.Data.SVCGlobal;
using static SirSqlValetCommands.Data.GCSS;
using static SirSqlValetCommands.Data.Extensions;

namespace SirSqlValetCommands
{
    public partial class FScript : Form
    {
        private bool throughMyShowDialog = false;

        public FScript()
        {
            InitializeComponent();

            splitContainer1.Panel1.BackColor    =   GCSS.SteelBlue_Dark.ToColor();
            lWarning.BackColor                  =   GCSS.SteelBlue_Dark.ToColor();

            lstScript.BackColor                 =   GCSS.SteelBlue_Dark.ToColor();
            lstScript.ForeColor                 =   Color.Black;
            lstScript.BorderStyle               =   BorderStyle.FixedSingle;

            bUndoAll.ForeColor                  =   GCSS.SteelBlue_Light.ToColor();
            bUndo.ForeColor                     =   GCSS.SteelBlue_Light.ToColor();
            bJoin.ForeColor                     =   GCSS.SteelBlue_Light.ToColor();
            bOK.ForeColor                       =   GCSS.SteelBlue_Light.ToColor();
            bCancel.ForeColor                   =   GCSS.SteelBlue_Light.ToColor();

            bUndoAll.BackColor                  =   GCSS.SteelBlue.ToColor();
            bUndo.BackColor                     =   GCSS.SteelBlue.ToColor();
            bJoin.BackColor                     =   GCSS.SteelBlue.ToColor();
            bOK.BackColor                       =   GCSS.SteelBlue.ToColor();
            bCancel.BackColor                   =   GCSS.SteelBlue.ToColor();

            throughMyShowDialog                 = false;

            AdjustUIContent();
        }

        public IEnumerable<string> MyShowDialog()
        {
            throughMyShowDialog = true;
            this.ShowDialog();
            throughMyShowDialog = false;
            return wd.scriptLines;
        }

        private void AdjustUIContent()
        {
            if (   bUndo.Visible != theStack.Count > 1)    bUndo.Visible = theStack.Count > 1;
            if (bUndoAll.Visible != theStack.Count > 2) bUndoAll.Visible = theStack.Count > 2;

            if (bUndoAll.Visible)
            {
                string newText = $"{bUndoAll.Tag} {theStack.Count - 1}";
                if (bUndoAll.Text != newText) bUndoAll.Text = newText;
            }

            lstScript.Items.Clear();
            foreach (var s in wd.scriptLines.FromToIdx(BOF, EOF))
                lstScript.Items.Add(s._);

            lstScript.SelectedIndex = Math.Min(lstScript.Items.Count, Math.Max(0, wd.numeroLigneCurseur));
            lstScript.Focus();
        }

        private void bCancel_Click(object sender, EventArgs e)
        {
            lWarning.Text = "";
            UndoAll();
            this.Close();
        }

        private void bOK_Click(object sender = null, EventArgs e = null)
        {
            lWarning.Text = "";
            this.Close();
        }

        private void bUndo_Click(object sender, EventArgs e)
        {
            lWarning.Text = "";
            UndoOne();
            AdjustUIContent();
        }

        private void bUndoAll_Click(object sender, EventArgs e)
        {
            lWarning.Text = "";
            UndoAll();
            AdjustUIContent();
        }

        private void bJoin_Click(object sender = null, EventArgs e = null)
        {
            lWarning.Text = "";
            wd.numeroLigneCurseur = lstScript.SelectedIndex;
            if (string.IsNullOrWhiteSpace((lWarning.Text = SirDBSidekickLogic.ProcessScriptAndSelectedLine())))
            {
                FJoin f = new FJoin();
                f.MyShowDialog();
                f.Dispose();
                f = null;
                AdjustUIContent();
                lstScript.Focus();
            }
        }

        private void lstScript_SelectedIndexChanged(object sender, EventArgs e)
        {
            lWarning.Text = "";
        }

        private void FScript_Shown(object sender, EventArgs e)
        {
            if (!throughMyShowDialog)
            {
                MessageBox.Show("Use MyShowDialog to display this window", "Sir Sql Valet", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                this.Close();
                return;
            }

            lWarning.Text = "";
            splitContainer1.Tag = splitContainer1.SplitterDistance.ToString();

            lstScript.Focus();

            var words = lstScript.SelectedItem.ToString().Split(new[] { ' ', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(_ => _.Trim());
            string selectedTable = "";
            try
            {
                selectedTable = words.Take(3).First(_ => BD_Schema.tables.Any(t => t.TABLE_NAME.Equals(_, nocase)));
            }
            catch
            {
            }
            if (selectedTable != string.Empty)
                bJoin_Click();
        }

        private void FScript_KeyDown(object sender, KeyEventArgs e)
        {
            lWarning.Text = "";
            if (e.KeyCode == Keys.Space)
                bOK_Click();
        }

        private void lstScript_DoubleClick(object sender, EventArgs e)
        {
            lWarning.Text = "";
            bJoin_Click();
        }
    }
}

