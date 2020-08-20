//-----------------------------------------------------------------------------
// GridControl.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using Microsoft.Xna.Framework;

namespace Xna.Tools
{
    /// <summary>
    /// This control sets control style for flicker free drawing.
    /// </summary>
    public partial class GridControl : UserControl
    {
        #region Constants
        const double MinGridSize = 50.0f;    // (pixel)
        const double MinGridScale = 1e-5;    //
        const double MaxGridScale = 1e3;
        const double MinOffset = -1e8;
        const double MaxOffset = 1e8;

        const float GridTextLeftMargin = 2.0f;
        const float GridTextBottomMargin = 2.0f;
        #endregion

        #region Properties

        [Category("Grid Control")]
        [Description("Turn on/off the grid.")]
        [DefaultValue(true)]
        public bool GridVisible
        {
            get { return gridVisible; }
            set { gridVisible= value; }
        }

        [Category("Grid Control")]
        [Description("The text color of grid.")]
        [DefaultValue(typeof(System.Drawing.Color), "Black")]
        public System.Drawing.Color GridTextColor
        {
            get { return gridTextColor; }
            set { gridTextColor = value; }
        }

        [Category("Grid Control")]
        [Description("The line color of grid.")]
        [DefaultValue(typeof(System.Drawing.Color), "200, 205, 211")]
        public System.Drawing.Color GridLineColor
        {
            get { return gridLineColor; }
            set { gridLineColor = value; }
        }

        [Category("Grid Control")]
        [Description("The bold line color of grid.")]
        [DefaultValue(typeof(System.Drawing.Color), "100, 102, 106")]
        public System.Drawing.Color GridBoldLineColor
        {
            get { return gridBoldLineColor; }
            set { gridBoldLineColor = value; }
        }

        [Browsable(false)]
        public Vector2 ViewSize { get { return viewSize; } }

        [Browsable(false)]
        public double OffsetX { get { return gridOffsets[0]; } }

        [Browsable(false)]
        public double OffsetY { get { return gridOffsets[1]; } }

        [Browsable(false)]
        public double ScaleX { get { return gridScale[0]; } }

        [Browsable(false)]
        public double ScaleY { get { return gridScale[1]; } }

        #endregion

        public GridControl()
        {
            InitializeComponent();

            // For flicker free animation
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);

            viewSize = new Vector2(Width, Height);
            gridScale[0] = 2.0 / viewSize.X;
            gridScale[1] = 2.0 / viewSize.Y;
            invGridScale[0] = 1.0 / gridScale[0];
            invGridScale[1] = 1.0 / gridScale[1];
            gridOffsets[0] = gridOffsets[1] = -1.0;
            CalcGridSpans();
        }

        #region Public Methods

        public Vector2 ToPixelCoordinate( float position, float value )
        {
            double x = ((double)position - gridOffsets[0]) * invGridScale[0];
            double y = (double)viewSize.Y -
                            ((double)value - gridOffsets[1]) * invGridScale[1];
            return new Vector2((float)x, (float)y);
        }

        public Vector2 ToPixelCoordinate(Vector2 unitPos)
        {
            return ToPixelCoordinate(unitPos.X, unitPos.Y);
        }

        public Vector2 ToUnitCoordinate( Vector2 pixelPos )
        {
            double x = gridOffsets[0] + (double)pixelPos.X * gridScale[0];
            double y = gridOffsets[1] + ((double)viewSize.Y -
                                        (double)pixelPos.Y) * gridScale[1];
            return new Vector2((float)x, (float)y);
        }

        public Vector2 ToUnitCoordinate(float xp, float yp)
        {
            double xu = gridOffsets[0] + (double)xp * gridScale[0];
            double yu = gridOffsets[1] +
                                    ((double)viewSize.Y - (double)yp) * gridScale[1];
            return new Vector2((float)xu, (float)yu);
        }

        public float UnitToScreenTangentAngle( float unitTangent )
        {
            return (float)((double)unitTangent / (gridScale[1] / gridScale[0]));
        }

        public void Pan(Vector2 dv)
        {
            SetOffset(gridOffsets[0] - (double)dv.X * gridScale[0],
                    gridOffsets[1] - (double)dv.Y * -gridScale[1]);
        }

        public void Zoom(Vector2 dv)
        {
            // Pick-up longest delta.
            double dx = (double)dv.X * gridScale[0];
            double dy = (double)dv.Y * -gridScale[1];
            double s = -0.001 * ((Math.Abs(dx) > Math.Abs(dy)) ? dx : dy);
            Zoom(s, s);
        }

        public void Zoom(int wheelDelta)
        {
            const int WHEEL_DELTA = 120;

            // Pick-up longest delta.
            float s = (float)wheelDelta / WHEEL_DELTA;
            double dx = s * 0.1 * gridScale[0];
            double dy = s * 0.1 * gridScale[1];
            Zoom(dx, dy);
        }

        public bool Frame(Vector2 min, Vector2 max)
        {
            bool result = false;

            // if min and max values are same will have 1.0 as new range.
            if ( min.X == max.X ) max.X = min.X + 1.0f;
            if ( min.Y == max.Y ) max.Y = min.Y + 1.0f;

            Vector2 newRange = Vector2.Max(max - min,
                            new Vector2((float)MinGridScale, (float)MinGridScale));

            float margin_ps = 50.0f;
            double[] newGridScale = {
                (double)newRange.X / (double)Math.Max(viewSize.X - margin_ps, 10.0f ),
                (double)newRange.Y / (double)Math.Max(viewSize.Y - margin_ps, 10.0f)};

            double threshold = 1e-4;
            if( Math.Abs( newGridScale[0] - gridScale[0] ) > threshold ||
                Math.Abs(newGridScale[1] - gridScale[1]) > threshold  )
            {
                SetScale(newGridScale[0], newGridScale[1]);
                SetOffset(
                    (double)min.X - (gridScale[0] * margin_ps * 0.5),
                    (double)min.Y - (gridScale[1] * margin_ps * 0.5));

                result = true;
            }

            return result;

        }

        public void DrawGridLines(Graphics g)
        {
            if (g == null) throw new ArgumentNullException("g");

            CalcGridSpans();

            if (gridVisible == false) return;

            Pen pen = new Pen(GridLineColor);
            Pen boldPen = new Pen(GridBoldLineColor);
            Brush brush = new SolidBrush(GridTextColor);

            // Compute Text height.
            float textHeight = g.MeasureString("-+123456789E", Font).Height;

            // Grid line start and end position in Unit Coordinate.
            double sx_us = gridOffsets[0] - (gridOffsets[0] % gridSpans[0]);
            double sy_us = gridOffsets[1] - (gridOffsets[1] % gridSpans[1]);
            double ex_us = sx_us + gridScale[0] * (double)viewSize.X;
            double ey_us = sy_us + gridScale[1] * (double)viewSize.Y;

            // Draw Vertical lines
            const double boldThreshold = 1e-8;
            for ( double p_us = sx_us; p_us < ex_us; p_us += gridSpans[0] )
            {
                int p_ps = (int)((p_us - gridOffsets[0]) * invGridScale[0]);
                bool isBoldLine = Math.Abs(p_us) < boldThreshold;
                g.DrawLine(isBoldLine ? boldPen : pen, p_ps, 0, p_ps, viewSize.Y);
            }

            // Draw Horizon lines
            for (double p_us = sy_us; p_us < ey_us; p_us += gridSpans[1])
            {
                int p_ps = (int)((p_us - gridOffsets[1]) * invGridScale[1]);
                int y = (int)viewSize.Y - p_ps;
                bool isBoldLine = Math.Abs(p_us) < boldThreshold;
                g.DrawLine(isBoldLine ? boldPen : pen, 0, y, viewSize.X, y);
            }

            // Draw Vertical line numbers
            float lastTextPos = -100.0f;
            for (double p_us = sx_us; p_us < ex_us; p_us += gridSpans[0])
            {
                int p_ps = (int)((p_us - gridOffsets[0]) * invGridScale[0]);

                // Make sure current text drawing over previous number text.
                String text = ToNumberString(p_us);
                SizeF size = g.MeasureString(text, Font);
                float hw = size.Width * 0.5f;
                float x = p_ps - hw;
                if (lastTextPos < x)
                {
                    lastTextPos = p_ps + hw;
                    g.DrawString(text, this.Font, brush,
                                x, viewSize.Y - size.Height - GridTextBottomMargin);
                }
            }

            // Draw Horizontal line numbers.
            float hh = textHeight * 0.5f;
            float bottomLimit = viewSize.Y - 2 * textHeight - GridTextBottomMargin;
            lastTextPos = viewSize.Y + 100.0f;
            for (double p_us = sy_us; p_us < ey_us; p_us += gridSpans[1])
            {
                int p_ps = (int)((p_us - gridOffsets[1]) * invGridScale[1]);

                // Make sure current text drawing over previous number text.
                float y = viewSize.Y - p_ps - hh;
                if (lastTextPos > y && y < bottomLimit)
                {
                    lastTextPos = y - hh;
                    g.DrawString(ToNumberString(p_us), this.Font, brush,
                                                            GridTextLeftMargin, y);
                }
            }

        }

        #endregion

        #region Private Methods

        private void Zoom(double dx, double dy)
        {
            double sx = gridScale[0], sy = gridScale[1];
            SetScale(gridScale[0] + dx, gridScale[1] + dy);

            dx = gridScale[0] - sx;
            dy = gridScale[1] - sy;

            gridOffsets[0] -= dx * (double)viewSize.X * 0.5;
            gridOffsets[1] -= dy * (double)viewSize.Y * 0.5;
        }

        private void CalcGridSpans()
        {
            if (viewSize.X >= MinGridSize)
                gridSpans[0] = ComputeGridSpan(MinGridSize * gridScale[0]);

            if (viewSize.Y >= MinGridSize)
                gridSpans[1] = ComputeGridSpan(MinGridSize * gridScale[1]);
        }

        private static string ToNumberString(double value)
        {
            double val = Math.Round(value, 9);
            return val.ToString();
        }

        private void GridControl_Resize(object sender, EventArgs e)
        {
            Vector2 newViewSize =
                Vector2.Max(new Vector2(Width, Height), Vector2.One * 10);

            Vector2 resizeScalar = viewSize / newViewSize;

            SetScale(gridScale[0] * (double)resizeScalar.X,
                        gridScale[1] * (double)resizeScalar.Y);
            viewSize = newViewSize;
            Invalidate(true);
        }

        static double[] _span_table = { 0.1, 0.2, 0.5, 1.0, 2.0, 4.0, 5.0 };

        static double ComputeGridSpan(double attendedSpan)
        {
            double digit = Math.Truncate(Math.Log10(attendedSpan));

            double baseNum = Math.Pow(10, digit);

            double minSpan = 0;
            double min = Double.MaxValue;
            for (int i = 0; i < _span_table.Length; ++i)
            {
                double span = baseNum * _span_table[i];
                double dis = Math.Abs(span - attendedSpan);
                if (dis < min)
                {
                    min = dis;
                    minSpan = span;
                }
            }

            return minSpan;
        }

        private void SetOffset(double x, double y)
        {
            gridOffsets[0] = Math.Min(Math.Max(x, MinOffset), MaxOffset);
            gridOffsets[1] = Math.Min(Math.Max(y, MinOffset), MaxOffset);
        }

        private void SetScale(double x, double y)
        {
            gridScale[0] = Math.Min(Math.Max(x, MinGridScale), MaxGridScale);
            gridScale[1] = Math.Min(Math.Max(y, MinGridScale), MaxGridScale);
            invGridScale[0] = 1.0 / gridScale[0];
            invGridScale[1] = 1.0 / gridScale[1];
        }

        #endregion

        #region Propertie wrapper members

        private System.Drawing.Color gridTextColor = System.Drawing.Color.Black;
        private System.Drawing.Color gridLineColor = System.Drawing.Color.FromArgb(200, 205, 211);
        private System.Drawing.Color gridBoldLineColor = System.Drawing.Color.FromArgb(100, 102, 106);
        private bool gridVisible = true;

        #endregion

        #region Private Members

        // Grid ralated params.
        private double[]    gridSpans       = {0.1,0.1};

        // unit  (bottom left corner of window)
        private double[]    gridOffsets     = {0,0};

        // unit/pixel
        private double[]    gridScale       = {0,0};

        // pixel/unit
        private double[]    invGridScale    = {0,0};

        private Vector2 viewSize = new Vector2();

        #endregion

    }
}
