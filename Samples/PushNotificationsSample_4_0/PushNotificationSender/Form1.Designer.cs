//-----------------------------------------------------------------------------
// Form1.Designer.cs
//
// Microsoft Advanced Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

namespace PushNotificationSender
{
    partial class Form1
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonSendRaw = new System.Windows.Forms.Button();
            this.rawText = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tileBackgroundImageUri = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tileCount = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tileTitle = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonSendTile = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.toastText2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.toastText1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonSendToast = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.phoneURI = new System.Windows.Forms.TextBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.serverResponse = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.buttonSendRaw);
            this.groupBox1.Controls.Add(this.rawText);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Location = new System.Drawing.Point(12, 333);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(395, 91);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Raw Notification";
            // 
            // buttonSendRaw
            // 
            this.buttonSendRaw.Location = new System.Drawing.Point(314, 62);
            this.buttonSendRaw.Name = "buttonSendRaw";
            this.buttonSendRaw.Size = new System.Drawing.Size(75, 23);
            this.buttonSendRaw.TabIndex = 17;
            this.buttonSendRaw.Text = "Send";
            this.buttonSendRaw.UseVisualStyleBackColor = true;
            this.buttonSendRaw.Click += new System.EventHandler(this.buttonSendRaw_Click);
            // 
            // rawText
            // 
            this.rawText.Location = new System.Drawing.Point(10, 32);
            this.rawText.Name = "rawText";
            this.rawText.Size = new System.Drawing.Size(378, 20);
            this.rawText.TabIndex = 16;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 16);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "Raw Text";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tileBackgroundImageUri);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.tileCount);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.tileTitle);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.buttonSendTile);
            this.groupBox2.Location = new System.Drawing.Point(12, 200);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(395, 127);
            this.groupBox2.TabIndex = 16;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Tile Notification";
            // 
            // tileBackgroundImageUri
            // 
            this.tileBackgroundImageUri.Location = new System.Drawing.Point(10, 71);
            this.tileBackgroundImageUri.Name = "tileBackgroundImageUri";
            this.tileBackgroundImageUri.Size = new System.Drawing.Size(378, 20);
            this.tileBackgroundImageUri.TabIndex = 16;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 55);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Background Image URI";
            // 
            // tileCount
            // 
            this.tileCount.Location = new System.Drawing.Point(336, 32);
            this.tileCount.Name = "tileCount";
            this.tileCount.ReadOnly = true;
            this.tileCount.Size = new System.Drawing.Size(53, 20);
            this.tileCount.TabIndex = 14;
            this.tileCount.Text = "1";
            this.tileCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(334, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(55, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Tile Count";
            // 
            // tileTitle
            // 
            this.tileTitle.Location = new System.Drawing.Point(10, 32);
            this.tileTitle.Name = "tileTitle";
            this.tileTitle.Size = new System.Drawing.Size(322, 20);
            this.tileTitle.TabIndex = 12;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Tile Title";
            // 
            // buttonSendTile
            // 
            this.buttonSendTile.Location = new System.Drawing.Point(313, 97);
            this.buttonSendTile.Name = "buttonSendTile";
            this.buttonSendTile.Size = new System.Drawing.Size(75, 23);
            this.buttonSendTile.TabIndex = 5;
            this.buttonSendTile.Text = "Send";
            this.buttonSendTile.UseVisualStyleBackColor = true;
            this.buttonSendTile.Click += new System.EventHandler(this.buttonSendTile_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.toastText2);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.toastText1);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.buttonSendToast);
            this.groupBox3.Location = new System.Drawing.Point(12, 65);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(395, 129);
            this.groupBox3.TabIndex = 17;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Toast Notification";
            // 
            // toastText2
            // 
            this.toastText2.Location = new System.Drawing.Point(9, 71);
            this.toastText2.Name = "toastText2";
            this.toastText2.Size = new System.Drawing.Size(381, 20);
            this.toastText2.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 55);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Toast Text 2";
            // 
            // toastText1
            // 
            this.toastText1.Location = new System.Drawing.Point(10, 32);
            this.toastText1.Name = "toastText1";
            this.toastText1.Size = new System.Drawing.Size(381, 20);
            this.toastText1.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Toast Text 1";
            // 
            // buttonSendToast
            // 
            this.buttonSendToast.Location = new System.Drawing.Point(313, 97);
            this.buttonSendToast.Name = "buttonSendToast";
            this.buttonSendToast.Size = new System.Drawing.Size(75, 23);
            this.buttonSendToast.TabIndex = 4;
            this.buttonSendToast.Text = "Send";
            this.buttonSendToast.UseVisualStyleBackColor = true;
            this.buttonSendToast.Click += new System.EventHandler(this.buttonSendToast_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.phoneURI);
            this.groupBox4.Location = new System.Drawing.Point(12, 12);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(398, 47);
            this.groupBox4.TabIndex = 18;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "URI of Windows Phone Device";
            // 
            // phoneURI
            // 
            this.phoneURI.Location = new System.Drawing.Point(13, 19);
            this.phoneURI.Name = "phoneURI";
            this.phoneURI.Size = new System.Drawing.Size(381, 20);
            this.phoneURI.TabIndex = 6;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.serverResponse);
            this.groupBox5.Location = new System.Drawing.Point(12, 430);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(394, 118);
            this.groupBox5.TabIndex = 19;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Microsoft Push Notification Server Response";
            // 
            // serverResponse
            // 
            this.serverResponse.Location = new System.Drawing.Point(9, 19);
            this.serverResponse.Multiline = true;
            this.serverResponse.Name = "serverResponse";
            this.serverResponse.ReadOnly = true;
            this.serverResponse.Size = new System.Drawing.Size(379, 93);
            this.serverResponse.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(418, 560);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "Push Notification Sender";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonSendRaw;
        private System.Windows.Forms.TextBox rawText;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox tileBackgroundImageUri;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tileCount;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tileTitle;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button buttonSendTile;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox toastText2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox toastText1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonSendToast;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox phoneURI;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TextBox serverResponse;

    }
}

