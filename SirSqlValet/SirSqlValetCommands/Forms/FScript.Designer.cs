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
    partial class FScript
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lWarning = new System.Windows.Forms.Label();
            this.bUndoAll = new System.Windows.Forms.Button();
            this.bUndo = new System.Windows.Forms.Button();
            this.bJoin = new System.Windows.Forms.Button();
            this.bOK = new System.Windows.Forms.Button();
            this.bCancel = new System.Windows.Forms.Button();
            this.panelScript = new System.Windows.Forms.Panel();
            this.lstScript = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panelScript.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(5);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lWarning);
            this.splitContainer1.Panel1.Controls.Add(this.bUndoAll);
            this.splitContainer1.Panel1.Controls.Add(this.bUndo);
            this.splitContainer1.Panel1.Controls.Add(this.bJoin);
            this.splitContainer1.Panel1.Controls.Add(this.bOK);
            this.splitContainer1.Panel1.Controls.Add(this.bCancel);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panelScript);
            this.splitContainer1.Size = new System.Drawing.Size(2172, 1082);
            this.splitContainer1.SplitterDistance = 123;
            this.splitContainer1.SplitterWidth = 2;
            this.splitContainer1.TabIndex = 8;
            this.splitContainer1.TabStop = false;
            // 
            // lWarning
            // 
            this.lWarning.AutoSize = true;
            this.lWarning.BackColor = System.Drawing.Color.Black;
            this.lWarning.Font = new System.Drawing.Font("Segoe UI Light", 13F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
            this.lWarning.ForeColor = System.Drawing.Color.Red;
            this.lWarning.Location = new System.Drawing.Point(15, 72);
            this.lWarning.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lWarning.Name = "lWarning";
            this.lWarning.Size = new System.Drawing.Size(303, 47);
            this.lWarning.TabIndex = 7;
            this.lWarning.Text = "warning goes here";
            // 
            // bUndoAll
            // 
            this.bUndoAll.BackColor = System.Drawing.Color.SteelBlue;
            this.bUndoAll.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.bUndoAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bUndoAll.Font = new System.Drawing.Font("Showcard Gothic", 12F);
            this.bUndoAll.ForeColor = System.Drawing.Color.Black;
            this.bUndoAll.Location = new System.Drawing.Point(666, 15);
            this.bUndoAll.Margin = new System.Windows.Forms.Padding(0);
            this.bUndoAll.Name = "bUndoAll";
            this.bUndoAll.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.bUndoAll.Size = new System.Drawing.Size(218, 50);
            this.bUndoAll.TabIndex = 4;
            this.bUndoAll.Tag = "UNDO &ALL";
            this.bUndoAll.Text = "UNDO &ALL 0";
            this.bUndoAll.UseVisualStyleBackColor = false;
            this.bUndoAll.Visible = false;
            this.bUndoAll.Click += new System.EventHandler(this.bUndoAll_Click);
            // 
            // bUndo
            // 
            this.bUndo.BackColor = System.Drawing.Color.SteelBlue;
            this.bUndo.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.bUndo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bUndo.Font = new System.Drawing.Font("Showcard Gothic", 12F);
            this.bUndo.ForeColor = System.Drawing.Color.Black;
            this.bUndo.Location = new System.Drawing.Point(518, 15);
            this.bUndo.Margin = new System.Windows.Forms.Padding(0);
            this.bUndo.Name = "bUndo";
            this.bUndo.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.bUndo.Size = new System.Drawing.Size(137, 50);
            this.bUndo.TabIndex = 3;
            this.bUndo.Text = "&UNDO";
            this.bUndo.UseVisualStyleBackColor = false;
            this.bUndo.Visible = false;
            this.bUndo.Click += new System.EventHandler(this.bUndo_Click);
            // 
            // bJoin
            // 
            this.bJoin.BackColor = System.Drawing.Color.SteelBlue;
            this.bJoin.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.bJoin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bJoin.Font = new System.Drawing.Font("Showcard Gothic", 12F);
            this.bJoin.ForeColor = System.Drawing.Color.Black;
            this.bJoin.Location = new System.Drawing.Point(334, 15);
            this.bJoin.Margin = new System.Windows.Forms.Padding(0);
            this.bJoin.Name = "bJoin";
            this.bJoin.Size = new System.Drawing.Size(137, 50);
            this.bJoin.TabIndex = 2;
            this.bJoin.Text = "&JOIN...";
            this.bJoin.UseVisualStyleBackColor = false;
            this.bJoin.Click += new System.EventHandler(this.bJoin_Click);
            // 
            // bOK
            // 
            this.bOK.BackColor = System.Drawing.Color.SteelBlue;
            this.bOK.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.bOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bOK.Font = new System.Drawing.Font("Showcard Gothic", 12F);
            this.bOK.ForeColor = System.Drawing.Color.Black;
            this.bOK.Location = new System.Drawing.Point(194, 15);
            this.bOK.Margin = new System.Windows.Forms.Padding(0);
            this.bOK.Name = "bOK";
            this.bOK.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.bOK.Size = new System.Drawing.Size(83, 50);
            this.bOK.TabIndex = 1;
            this.bOK.Text = "&OK";
            this.bOK.UseVisualStyleBackColor = false;
            this.bOK.Click += new System.EventHandler(this.bOK_Click);
            // 
            // bCancel
            // 
            this.bCancel.BackColor = System.Drawing.Color.SteelBlue;
            this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bCancel.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.bCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bCancel.Font = new System.Drawing.Font("Showcard Gothic", 12F);
            this.bCancel.ForeColor = System.Drawing.Color.Black;
            this.bCancel.Location = new System.Drawing.Point(15, 15);
            this.bCancel.Margin = new System.Windows.Forms.Padding(0);
            this.bCancel.Name = "bCancel";
            this.bCancel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.bCancel.Size = new System.Drawing.Size(163, 50);
            this.bCancel.TabIndex = 0;
            this.bCancel.Text = "CANC&EL";
            this.bCancel.UseVisualStyleBackColor = false;
            this.bCancel.Click += new System.EventHandler(this.bCancel_Click);
            // 
            // panelScript
            // 
            this.panelScript.Controls.Add(this.lstScript);
            this.panelScript.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelScript.Location = new System.Drawing.Point(0, 0);
            this.panelScript.Margin = new System.Windows.Forms.Padding(5);
            this.panelScript.Name = "panelScript";
            this.panelScript.Size = new System.Drawing.Size(2172, 957);
            this.panelScript.TabIndex = 3;
            // 
            // lstScript
            // 
            this.lstScript.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(25)))));
            this.lstScript.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstScript.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstScript.Font = new System.Drawing.Font("Consolas", 13.875F);
            this.lstScript.ForeColor = System.Drawing.Color.LightSteelBlue;
            this.lstScript.FormattingEnabled = true;
            this.lstScript.IntegralHeight = false;
            this.lstScript.ItemHeight = 43;
            this.lstScript.Location = new System.Drawing.Point(0, 0);
            this.lstScript.Margin = new System.Windows.Forms.Padding(0);
            this.lstScript.Name = "lstScript";
            this.lstScript.Size = new System.Drawing.Size(2172, 957);
            this.lstScript.TabIndex = 6;
            this.lstScript.SelectedIndexChanged += new System.EventHandler(this.lstScript_SelectedIndexChanged);
            this.lstScript.DoubleClick += new System.EventHandler(this.lstScript_DoubleClick);
            // 
            // FScript
            // 
            this.AcceptButton = this.bJoin;
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.CancelButton = this.bCancel;
            this.ClientSize = new System.Drawing.Size(2172, 1082);
            this.Controls.Add(this.splitContainer1);
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MinimizeBox = false;
            this.Name = "FScript";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sir Sql Valet";
            this.Shown += new System.EventHandler(this.FScript_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FScript_KeyDown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panelScript.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private SplitContainer splitContainer1;
        private Label lWarning;
        private Button bUndoAll;
        private Button bUndo;
        private Button bJoin;
        private Button bOK;
        private Button bCancel;
        private Panel panelScript;
        private ListBox lstScript;
    }
}
