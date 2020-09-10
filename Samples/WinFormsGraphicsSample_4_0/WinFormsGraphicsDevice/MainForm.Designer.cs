namespace WinFormsGraphicsDevice
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.spriteFontControl = new WinFormsGraphicsDevice.SpriteFontControl();
            this.vertexColor3 = new System.Windows.Forms.ComboBox();
            this.vertexColor2 = new System.Windows.Forms.ComboBox();
            this.vertexColor1 = new System.Windows.Forms.ComboBox();
            this.spinningTriangleControl = new WinFormsGraphicsDevice.SpinningTriangleControl();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.spriteFontControl);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.vertexColor3);
            this.splitContainer1.Panel2.Controls.Add(this.vertexColor2);
            this.splitContainer1.Panel2.Controls.Add(this.vertexColor1);
            this.splitContainer1.Panel2.Controls.Add(this.spinningTriangleControl);
            this.splitContainer1.Size = new System.Drawing.Size(792, 573);
            this.splitContainer1.SplitterDistance = 396;
            this.splitContainer1.TabIndex = 0;
            // 
            // spriteFontControl
            // 
            this.spriteFontControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spriteFontControl.Location = new System.Drawing.Point(0, 0);
            this.spriteFontControl.Name = "spriteFontControl";
            this.spriteFontControl.Size = new System.Drawing.Size(396, 573);
            this.spriteFontControl.TabIndex = 0;
            this.spriteFontControl.Text = "spriteFontControl";
            // 
            // vertexColor3
            // 
            this.vertexColor3.DropDownHeight = 500;
            this.vertexColor3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.vertexColor3.FormattingEnabled = true;
            this.vertexColor3.IntegralHeight = false;
            this.vertexColor3.Items.AddRange(new object[] {
            "BurlyWood",
            "Chartreuse",
            "Coral",
            "CornflowerBlue",
            "Cornsilk",
            "Firebrick",
            "Fuchsia",
            "Goldenrod",
            "Indigo",
            "Tan",
            "Teal",
            "Thistle",
            "Tomato"});
            this.vertexColor3.Location = new System.Drawing.Point(234, 12);
            this.vertexColor3.Name = "vertexColor3";
            this.vertexColor3.Size = new System.Drawing.Size(103, 21);
            this.vertexColor3.TabIndex = 3;
            this.vertexColor3.SelectedIndexChanged += new System.EventHandler(this.vertexColor_SelectedIndexChanged);
            // 
            // vertexColor2
            // 
            this.vertexColor2.DropDownHeight = 500;
            this.vertexColor2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.vertexColor2.FormattingEnabled = true;
            this.vertexColor2.IntegralHeight = false;
            this.vertexColor2.Items.AddRange(new object[] {
            "BurlyWood",
            "Chartreuse",
            "Coral",
            "CornflowerBlue",
            "Cornsilk",
            "Firebrick",
            "Fuchsia",
            "Goldenrod",
            "Indigo",
            "Tan",
            "Teal",
            "Thistle",
            "Tomato"});
            this.vertexColor2.Location = new System.Drawing.Point(125, 12);
            this.vertexColor2.Name = "vertexColor2";
            this.vertexColor2.Size = new System.Drawing.Size(103, 21);
            this.vertexColor2.TabIndex = 2;
            this.vertexColor2.SelectedIndexChanged += new System.EventHandler(this.vertexColor_SelectedIndexChanged);
            // 
            // vertexColor1
            // 
            this.vertexColor1.DropDownHeight = 500;
            this.vertexColor1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.vertexColor1.FormattingEnabled = true;
            this.vertexColor1.IntegralHeight = false;
            this.vertexColor1.Items.AddRange(new object[] {
            "BurlyWood",
            "Chartreuse",
            "Coral",
            "CornflowerBlue",
            "Cornsilk",
            "Firebrick",
            "Fuchsia",
            "Goldenrod",
            "Indigo",
            "Tan",
            "Teal",
            "Thistle",
            "Tomato"});
            this.vertexColor1.Location = new System.Drawing.Point(16, 12);
            this.vertexColor1.Name = "vertexColor1";
            this.vertexColor1.Size = new System.Drawing.Size(103, 21);
            this.vertexColor1.TabIndex = 1;
            this.vertexColor1.SelectedIndexChanged += new System.EventHandler(this.vertexColor_SelectedIndexChanged);
            // 
            // spinningTriangleControl
            // 
            this.spinningTriangleControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spinningTriangleControl.Location = new System.Drawing.Point(0, 0);
            this.spinningTriangleControl.Name = "spinningTriangleControl";
            this.spinningTriangleControl.Size = new System.Drawing.Size(392, 573);
            this.spinningTriangleControl.TabIndex = 0;
            this.spinningTriangleControl.Text = "spinningTriangleControl";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(792, 573);
            this.Controls.Add(this.splitContainer1);
            this.Name = "MainForm";
            this.Text = "WinForms Graphics Device";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private SpriteFontControl spriteFontControl;
        private SpinningTriangleControl spinningTriangleControl;
        private System.Windows.Forms.ComboBox vertexColor1;
        private System.Windows.Forms.ComboBox vertexColor3;
        private System.Windows.Forms.ComboBox vertexColor2;
    }
}

