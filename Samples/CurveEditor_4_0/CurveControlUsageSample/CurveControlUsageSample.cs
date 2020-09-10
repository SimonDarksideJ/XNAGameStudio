//-----------------------------------------------------------------------------
// CurveControlUsageSample.cs
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

using Microsoft.Xna.Framework;
using Xna.Tools;

namespace Xna.Samples
{
    public partial class CurveControlUsageSample : Form
    {
        public CurveControlUsageSample()
        {
            InitializeComponent();

            // Add Curve1 to curveControl1 that is editable.
            Curve curve1 = new Curve();
            curve1.Keys.Add(new CurveKey(0, 0));
            curve1.Keys.Add(new CurveKey(0.5f, 1));
            curve1.Keys.Add(new CurveKey(1, 0));

            curveControl1.Curves.Add(new EditCurve("Curve1", System.Drawing.Color.Red, curve1,
                                            CommandHistory.EnsureHasService(Site)));
            curveControl1.FrameAll();

            // Add Curve2 to curveControl2 that is non editable.
            Curve curve2 = new Curve();
            curve2.Keys.Add(new CurveKey(0, 0));
            curve2.Keys.Add(new CurveKey(0.5f, -1));
            curve2.Keys.Add(new CurveKey(1, 0));
            curveControl2.Curves.Add(new EditCurve("Curve2", System.Drawing.Color.Green, curve2,
                                            CommandHistory.EnsureHasService(Site)));
            curveControl2.FrameAll();
        }
    }
}