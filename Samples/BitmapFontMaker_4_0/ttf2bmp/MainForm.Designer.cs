//-----------------------------------------------------------------------------
// MainForm.Designer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

namespace TrueTypeConverter
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.FontName = new System.Windows.Forms.ComboBox();
            this.FontStyle = new System.Windows.Forms.ComboBox();
            this.FontSize = new System.Windows.Forms.ComboBox();
            this.Antialias = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.Sample = new System.Windows.Forms.Label();
            this.Export = new System.Windows.Forms.Button();
            this.MaxChar = new System.Windows.Forms.TextBox();
            this.MinChar = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // FontName
            // 
            this.FontName.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.FontName.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.FontName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple;
            this.FontName.FormattingEnabled = true;
            this.FontName.Location = new System.Drawing.Point(15, 30);
            this.FontName.Name = "FontName";
            this.FontName.Size = new System.Drawing.Size(189, 176);
            this.FontName.TabIndex = 1;
            this.FontName.SelectedIndexChanged += new System.EventHandler(this.FontName_SelectedIndexChanged);
            // 
            // FontStyle
            // 
            this.FontStyle.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.FontStyle.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.FontStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple;
            this.FontStyle.FormattingEnabled = true;
            this.FontStyle.Items.AddRange(new object[] {
            "Regular",
            "Italic",
            "Bold",
            "Bold, Italic"});
            this.FontStyle.Location = new System.Drawing.Point(218, 30);
            this.FontStyle.Name = "FontStyle";
            this.FontStyle.Size = new System.Drawing.Size(80, 176);
            this.FontStyle.TabIndex = 3;
            this.FontStyle.Text = "Regular";
            this.FontStyle.SelectedIndexChanged += new System.EventHandler(this.FontStyle_SelectedIndexChanged);
            // 
            // FontSize
            // 
            this.FontSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple;
            this.FontSize.FormattingEnabled = true;
            this.FontSize.Items.AddRange(new object[] {
            "8",
            "9",
            "10",
            "11",
            "12",
            "14",
            "16",
            "18",
            "20",
            "22",
            "23",
            "24",
            "26",
            "28",
            "36",
            "48",
            "72"});
            this.FontSize.Location = new System.Drawing.Point(312, 30);
            this.FontSize.Name = "FontSize";
            this.FontSize.Size = new System.Drawing.Size(49, 176);
            this.FontSize.TabIndex = 5;
            this.FontSize.Text = "23";
            this.FontSize.SelectedIndexChanged += new System.EventHandler(this.FontSize_SelectedIndexChanged);
            this.FontSize.TextUpdate += new System.EventHandler(this.FontSize_TextUpdate);
            // 
            // Antialias
            // 
            this.Antialias.AutoSize = true;
            this.Antialias.Checked = true;
            this.Antialias.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Antialias.Location = new System.Drawing.Point(182, 234);
            this.Antialias.Name = "Antialias";
            this.Antialias.Size = new System.Drawing.Size(77, 17);
            this.Antialias.TabIndex = 10;
            this.Antialias.Text = "&Antialiased";
            this.Antialias.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 215);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "M&in char:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(215, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Font s&tyle:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(310, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "&Size:";
            // 
            // Sample
            // 
            this.Sample.AutoEllipsis = true;
            this.Sample.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Sample.Location = new System.Drawing.Point(12, 269);
            this.Sample.Name = "Sample";
            this.Sample.Size = new System.Drawing.Size(354, 172);
            this.Sample.TabIndex = 12;
            this.Sample.Text = "The quick brown fox jumped over the LAZY camel";
            this.Sample.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Export
            // 
            this.Export.Location = new System.Drawing.Point(286, 228);
            this.Export.Name = "Export";
            this.Export.Size = new System.Drawing.Size(75, 23);
            this.Export.TabIndex = 11;
            this.Export.Text = "&Export";
            this.Export.UseVisualStyleBackColor = true;
            this.Export.Click += new System.EventHandler(this.Export_Click);
            // 
            // MaxChar
            // 
            this.MaxChar.Location = new System.Drawing.Point(97, 231);
            this.MaxChar.Name = "MaxChar";
            this.MaxChar.Size = new System.Drawing.Size(63, 20);
            this.MaxChar.TabIndex = 9;
            this.MaxChar.Text = "0x7F";
            // 
            // MinChar
            // 
            this.MinChar.Location = new System.Drawing.Point(15, 231);
            this.MinChar.Name = "MinChar";
            this.MinChar.Size = new System.Drawing.Size(63, 20);
            this.MinChar.TabIndex = 7;
            this.MinChar.Text = "0x20";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(94, 215);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Ma&x char:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 14);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(31, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "&Font:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(378, 450);
            this.Controls.Add(this.MinChar);
            this.Controls.Add(this.MaxChar);
            this.Controls.Add(this.Export);
            this.Controls.Add(this.Sample);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Antialias);
            this.Controls.Add(this.FontSize);
            this.Controls.Add(this.FontStyle);
            this.Controls.Add(this.FontName);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "ttf2bmp";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox FontName;
        private System.Windows.Forms.ComboBox FontStyle;
        private System.Windows.Forms.ComboBox FontSize;
        private System.Windows.Forms.CheckBox Antialias;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label Sample;
        private System.Windows.Forms.Button Export;
        private System.Windows.Forms.TextBox MaxChar;
        private System.Windows.Forms.TextBox MinChar;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
    }
}

