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
    partial class FJoin
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splMain = new System.Windows.Forms.SplitContainer();
            this.bJoin = new System.Windows.Forms.Button();
            this.bCancel = new System.Windows.Forms.Button();
            this.splLeft = new System.Windows.Forms.SplitContainer();
            this.listBox4 = new System.Windows.Forms.ListBox();
            this.splMiddle = new System.Windows.Forms.SplitContainer();
            this.splMiddleTop = new System.Windows.Forms.SplitContainer();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.splMiddleMiddle = new System.Windows.Forms.SplitContainer();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.listBox3 = new System.Windows.Forms.ListBox();
            this.listBox5 = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.splMain)).BeginInit();
            this.splMain.Panel1.SuspendLayout();
            this.splMain.Panel2.SuspendLayout();
            this.splMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splLeft)).BeginInit();
            this.splLeft.Panel1.SuspendLayout();
            this.splLeft.Panel2.SuspendLayout();
            this.splLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splMiddle)).BeginInit();
            this.splMiddle.Panel1.SuspendLayout();
            this.splMiddle.Panel2.SuspendLayout();
            this.splMiddle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splMiddleTop)).BeginInit();
            this.splMiddleTop.Panel1.SuspendLayout();
            this.splMiddleTop.Panel2.SuspendLayout();
            this.splMiddleTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splMiddleMiddle)).BeginInit();
            this.splMiddleMiddle.Panel1.SuspendLayout();
            this.splMiddleMiddle.Panel2.SuspendLayout();
            this.splMiddleMiddle.SuspendLayout();
            this.SuspendLayout();
            // 
            // splMain
            // 
            this.splMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splMain.Location = new System.Drawing.Point(0, 0);
            this.splMain.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.splMain.Name = "splMain";
            this.splMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splMain.Panel1
            // 
            this.splMain.Panel1.Controls.Add(this.bJoin);
            this.splMain.Panel1.Controls.Add(this.bCancel);
            // 
            // splMain.Panel2
            // 
            this.splMain.Panel2.Controls.Add(this.splLeft);
            this.splMain.Size = new System.Drawing.Size(2196, 1313);
            this.splMain.SplitterDistance = 104;
            this.splMain.SplitterWidth = 2;
            this.splMain.TabIndex = 0;
            this.splMain.TabStop = false;
            // 
            // bJoin
            // 
            this.bJoin.BackColor = System.Drawing.Color.SteelBlue;
            this.bJoin.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.bJoin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bJoin.Font = new System.Drawing.Font("Showcard Gothic", 12F);
            this.bJoin.ForeColor = System.Drawing.Color.Black;
            this.bJoin.Location = new System.Drawing.Point(207, 28);
            this.bJoin.Margin = new System.Windows.Forms.Padding(0);
            this.bJoin.Name = "bJoin";
            this.bJoin.Size = new System.Drawing.Size(117, 50);
            this.bJoin.TabIndex = 1;
            this.bJoin.Text = "&JOIN";
            this.bJoin.UseVisualStyleBackColor = false;
            this.bJoin.Click += new System.EventHandler(this.bJoin_Click);
            // 
            // bCancel
            // 
            this.bCancel.BackColor = System.Drawing.Color.SteelBlue;
            this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bCancel.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.bCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bCancel.Font = new System.Drawing.Font("Showcard Gothic", 12F);
            this.bCancel.ForeColor = System.Drawing.Color.Black;
            this.bCancel.Location = new System.Drawing.Point(15, 28);
            this.bCancel.Margin = new System.Windows.Forms.Padding(0);
            this.bCancel.Name = "bCancel";
            this.bCancel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.bCancel.Size = new System.Drawing.Size(179, 50);
            this.bCancel.TabIndex = 0;
            this.bCancel.Text = "CANC&EL";
            this.bCancel.UseVisualStyleBackColor = false;
            this.bCancel.Click += new System.EventHandler(this.bCancel_Click);
            // 
            // splLeft
            // 
            this.splLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splLeft.Location = new System.Drawing.Point(0, 0);
            this.splLeft.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.splLeft.Name = "splLeft";
            // 
            // splLeft.Panel1
            // 
            this.splLeft.Panel1.BackColor = System.Drawing.Color.Black;
            this.splLeft.Panel1.Controls.Add(this.listBox4);
            // 
            // splLeft.Panel2
            // 
            this.splLeft.Panel2.Controls.Add(this.splMiddle);
            this.splLeft.Size = new System.Drawing.Size(2196, 1207);
            this.splLeft.SplitterDistance = 685;
            this.splLeft.SplitterWidth = 2;
            this.splLeft.TabIndex = 0;
            this.splLeft.TabStop = false;
            // 
            // listBox4
            // 
            this.listBox4.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.listBox4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox4.Font = new System.Drawing.Font("Consolas", 9F);
            this.listBox4.IntegralHeight = false;
            this.listBox4.ItemHeight = 28;
            this.listBox4.Location = new System.Drawing.Point(0, 0);
            this.listBox4.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.listBox4.Name = "listBox4";
            this.listBox4.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.listBox4.Size = new System.Drawing.Size(685, 1207);
            this.listBox4.TabIndex = 1;
            this.listBox4.TabStop = false;
            this.listBox4.UseTabStops = false;
            // 
            // splMiddle
            // 
            this.splMiddle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splMiddle.Location = new System.Drawing.Point(0, 0);
            this.splMiddle.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.splMiddle.Name = "splMiddle";
            // 
            // splMiddle.Panel1
            // 
            this.splMiddle.Panel1.BackColor = System.Drawing.Color.Black;
            this.splMiddle.Panel1.Controls.Add(this.splMiddleTop);
            // 
            // splMiddle.Panel2
            // 
            this.splMiddle.Panel2.BackColor = System.Drawing.Color.Black;
            this.splMiddle.Panel2.Controls.Add(this.listBox5);
            this.splMiddle.Size = new System.Drawing.Size(1509, 1207);
            this.splMiddle.SplitterDistance = 857;
            this.splMiddle.SplitterWidth = 2;
            this.splMiddle.TabIndex = 0;
            this.splMiddle.TabStop = false;
            // 
            // splMiddleTop
            // 
            this.splMiddleTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splMiddleTop.Location = new System.Drawing.Point(0, 0);
            this.splMiddleTop.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.splMiddleTop.Name = "splMiddleTop";
            this.splMiddleTop.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splMiddleTop.Panel1
            // 
            this.splMiddleTop.Panel1.Controls.Add(this.listBox1);
            // 
            // splMiddleTop.Panel2
            // 
            this.splMiddleTop.Panel2.Controls.Add(this.splMiddleMiddle);
            this.splMiddleTop.Size = new System.Drawing.Size(857, 1207);
            this.splMiddleTop.SplitterDistance = 320;
            this.splMiddleTop.SplitterWidth = 2;
            this.splMiddleTop.TabIndex = 0;
            this.splMiddleTop.TabStop = false;
            // 
            // listBox1
            // 
            this.listBox1.BackColor = System.Drawing.Color.DarkGray;
            this.listBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox1.Font = new System.Drawing.Font("Consolas", 11F);
            this.listBox1.IntegralHeight = false;
            this.listBox1.ItemHeight = 34;
            this.listBox1.Location = new System.Drawing.Point(0, 0);
            this.listBox1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(857, 320);
            this.listBox1.TabIndex = 2;
            this.listBox1.UseTabStops = false;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            this.listBox1.DoubleClick += new System.EventHandler(this.listBox123_DoubleClick);
            this.listBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBox1_KeyDown);
            // 
            // splMiddleMiddle
            // 
            this.splMiddleMiddle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splMiddleMiddle.Location = new System.Drawing.Point(0, 0);
            this.splMiddleMiddle.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.splMiddleMiddle.Name = "splMiddleMiddle";
            this.splMiddleMiddle.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splMiddleMiddle.Panel1
            // 
            this.splMiddleMiddle.Panel1.BackColor = System.Drawing.Color.Black;
            this.splMiddleMiddle.Panel1.Controls.Add(this.listBox2);
            // 
            // splMiddleMiddle.Panel2
            // 
            this.splMiddleMiddle.Panel2.Controls.Add(this.listBox3);
            this.splMiddleMiddle.Size = new System.Drawing.Size(857, 885);
            this.splMiddleMiddle.SplitterDistance = 560;
            this.splMiddleMiddle.SplitterWidth = 2;
            this.splMiddleMiddle.TabIndex = 0;
            this.splMiddleMiddle.TabStop = false;
            // 
            // listBox2
            // 
            this.listBox2.BackColor = System.Drawing.Color.Gray;
            this.listBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox2.Font = new System.Drawing.Font("Consolas", 11F);
            this.listBox2.IntegralHeight = false;
            this.listBox2.ItemHeight = 34;
            this.listBox2.Location = new System.Drawing.Point(0, 0);
            this.listBox2.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.listBox2.Name = "listBox2";
            this.listBox2.Size = new System.Drawing.Size(857, 560);
            this.listBox2.TabIndex = 3;
            this.listBox2.SelectedIndexChanged += new System.EventHandler(this.listBox2_SelectedIndexChanged);
            this.listBox2.DoubleClick += new System.EventHandler(this.listBox123_DoubleClick);
            this.listBox2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBox2_KeyDown);
            // 
            // listBox3
            // 
            this.listBox3.BackColor = System.Drawing.Color.DarkGray;
            this.listBox3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox3.Font = new System.Drawing.Font("Consolas", 11F);
            this.listBox3.IntegralHeight = false;
            this.listBox3.ItemHeight = 34;
            this.listBox3.Location = new System.Drawing.Point(0, 0);
            this.listBox3.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.listBox3.Name = "listBox3";
            this.listBox3.Size = new System.Drawing.Size(857, 323);
            this.listBox3.TabIndex = 4;
            this.listBox3.SelectedIndexChanged += new System.EventHandler(this.listBox3_SelectedIndexChanged);
            this.listBox3.DoubleClick += new System.EventHandler(this.listBox123_DoubleClick);
            this.listBox3.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBox3_KeyDown);
            // 
            // listBox5
            // 
            this.listBox5.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.listBox5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listBox5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox5.Font = new System.Drawing.Font("Consolas", 9F);
            this.listBox5.IntegralHeight = false;
            this.listBox5.ItemHeight = 28;
            this.listBox5.Location = new System.Drawing.Point(0, 0);
            this.listBox5.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.listBox5.Name = "listBox5";
            this.listBox5.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.listBox5.Size = new System.Drawing.Size(650, 1207);
            this.listBox5.TabIndex = 1;
            this.listBox5.TabStop = false;
            // 
            // FJoin
            // 
            this.AcceptButton = this.bJoin;
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.Black;
            this.CancelButton = this.bCancel;
            this.ClientSize = new System.Drawing.Size(2196, 1313);
            this.Controls.Add(this.splMain);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.MinimizeBox = false;
            this.Name = "FJoin";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select a table to join to";
            this.Shown += new System.EventHandler(this.FJoin_Shown);
            this.Resize += new System.EventHandler(this.FJoin_Resize);
            this.splMain.Panel1.ResumeLayout(false);
            this.splMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splMain)).EndInit();
            this.splMain.ResumeLayout(false);
            this.splLeft.Panel1.ResumeLayout(false);
            this.splLeft.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splLeft)).EndInit();
            this.splLeft.ResumeLayout(false);
            this.splMiddle.Panel1.ResumeLayout(false);
            this.splMiddle.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splMiddle)).EndInit();
            this.splMiddle.ResumeLayout(false);
            this.splMiddleTop.Panel1.ResumeLayout(false);
            this.splMiddleTop.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splMiddleTop)).EndInit();
            this.splMiddleTop.ResumeLayout(false);
            this.splMiddleMiddle.Panel1.ResumeLayout(false);
            this.splMiddleMiddle.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splMiddleMiddle)).EndInit();
            this.splMiddleMiddle.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private SplitContainer splMain;
        private Button bCancel;
        private Button bJoin;
        private SplitContainer splLeft;
        private SplitContainer splMiddle;
        private SplitContainer splMiddleTop;
        private SplitContainer splMiddleMiddle;
        private ListBox listBox4;
        private ListBox listBox1;
        private ListBox listBox2;
        private ListBox listBox3;
        private ListBox listBox5;
    }
}