//-----------------------------------------------------------------------------
// HelpAbout.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Xna.Tools
{
    public partial class HelpAbout : Form
    {
        public HelpAbout()
        {
            InitializeComponent();
        }

        private void HelpAbout_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Enter)
                Close();
        }
    }
}