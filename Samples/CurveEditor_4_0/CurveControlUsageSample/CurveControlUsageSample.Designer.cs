//-----------------------------------------------------------------------------
// CurveControlUsageSample.Designer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
namespace Xna.Samples
{
    partial class CurveControlUsageSample
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
            this.curveControl1 = new Xna.Tools.CurveControl();
            this.label1 = new System.Windows.Forms.Label();
            this.curveControl2 = new Xna.Tools.CurveControl();
            this.label2 = new System.Windows.Forms.Label();
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
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.curveControl1);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.curveControl2);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Size = new System.Drawing.Size(491, 580);
            this.splitContainer1.SplitterDistance = 289;
            this.splitContainer1.TabIndex = 0;
            // 
            // curveControl1
            // 
            this.curveControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.curveControl1.Location = new System.Drawing.Point(0, 33);
            this.curveControl1.Name = "curveControl1";
            this.curveControl1.Size = new System.Drawing.Size(491, 256);
            this.curveControl1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(10);
            this.label1.Size = new System.Drawing.Size(132, 33);
            this.label1.TabIndex = 0;
            this.label1.Text = "Editable Curve Control";
            // 
            // curveControl2
            // 
            this.curveControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.curveControl2.Editable = false;
            this.curveControl2.Location = new System.Drawing.Point(0, 33);
            this.curveControl2.MenuVisible = false;
            this.curveControl2.Name = "curveControl2";
            this.curveControl2.Size = new System.Drawing.Size(491, 254);
            this.curveControl2.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Padding = new System.Windows.Forms.Padding(10);
            this.label2.Size = new System.Drawing.Size(152, 33);
            this.label2.TabIndex = 0;
            this.label2.Text = "Non-Editable CurveControl";
            // 
            // CurveControlUsageSample
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(491, 580);
            this.Controls.Add(this.splitContainer1);
            this.Name = "CurveControlUsageSample";
            this.Text = "CurveControl Usage Sample";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private Xna.Tools.CurveControl curveControl1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private Xna.Tools.CurveControl curveControl2;

    }
}

