#region File Description
//-----------------------------------------------------------------------------
// CurveControl.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

//-----------------------------------------------------------------------------
// CurveControl.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#region Using Statements
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.Xna.Framework;

#endregion



namespace Xna.Tools
{
    public partial class CurveControl : UserControl
    {
        #region Constants
        const float MinSelectSize = 16;
        const float TangentManipulatorLength = 50.0f;
        const float CurvePenWidth = 1.0f;
        const float MaximumCurveKeyEntry = (float)0x4000007F;
        #endregion

        #region Properties
        [Category("Curve Control")]
        [Description("Turn on or off menu in Curve Control.")]
        [DefaultValue(true)]
        public bool MenuVisible
        {
            get { return menuStrip.Visible; }
            set { menuStrip.Visible = toolStrip.Visible = value; }
        }

        [Category("Curve Control")]
        [Description("Turn on/off the grid.")]
        [DefaultValue(true)]
        public bool GridVisible
        {
            get { return curveView.GridVisible; }
            set { curveView.GridVisible = value; }
        }

        [Category("Curve Control")]
        [Description("Turn on or off menu in Curve editing.")]
        [DefaultValue(true)]
        public bool Editable { get { return editable; } set { editable = value; } }

        [Category("Curve Control")]
        [Description("The selection box fill color.")]
        [DefaultValue(typeof(System.Drawing.Color), "80, 0, 200, 128")]
        public System.Drawing.Color SelectingBoxColor
        {
            get { return selectingBoxColor; }
            set { selectingBoxColor = value; RequestRender(); }
        }

        [Category("Curve Control")]
        [Description("The selection box border color.")]
        [DefaultValue(typeof(System.Drawing.Color), "Black")]
        public System.Drawing.Color SelectingBoxBorderColor
        {
            get { return selectingBoxBorderColor; }
            set { selectingBoxBorderColor = value; RequestRender(); }
        }

        [Category("Curve Control")]
        [Description("The text color of grid.")]
        [DefaultValue(typeof(System.Drawing.Color), "Black")]
        public System.Drawing.Color GridTextColor
        {
            get { return curveView.GridTextColor; }
            set { curveView.GridTextColor = value; }
        }

        [Category("Curve Control")]
        [Description("The line color of grid.")]
        [DefaultValue(typeof(System.Drawing.Color), "200, 205, 211")]
        public System.Drawing.Color GridLineColor
        {
            get { return curveView.GridLineColor; }
            set { curveView.GridLineColor = value; }
        }

        [Category("Curve Control")]
        [Description("The bold line color of grid.")]
        [DefaultValue(typeof(System.Drawing.Color), "100, 102, 106")]
        public System.Drawing.Color GridBoldLineColor
        {
            get { return curveView.GridBoldLineColor; }
            set { curveView.GridBoldLineColor = value; }
        }

        [Browsable(false)]
        public EditCurveCollection Curves { get { return curves; } }

        #endregion

        #region Constructor
        public CurveControl()
        {
            curves = new EditCurveCollection();
            curves.Changed += new EventHandler(curves_Changed);
            curves.AddingCurve +=
                new EventHandler<EditCurveEventArgs>(curves_AddingCurve);

            curves.RemovingCurve +=
                new EventHandler<EditCurveEventArgs>(curves_RemovingCurve);

            InitializeComponent();

            //
            editButtons = new ToolStripButton[]
            {
                selectionButton,
                addKeyButton,
                moveKeyButton,
                panButton,
                zoomButton,
            };

            //
            quickEditKeyMap = new Dictionary<Keys, EditMode>();
            quickEditKeyMap.Add(Keys.Space, EditMode.Pan);
            //
            editKeyMap = new Dictionary<Keys, EditMode>();
            editKeyMap.Add(Keys.A, EditMode.Add);
            editKeyMap.Add(Keys.S, EditMode.Select);
            editKeyMap.Add(Keys.D, EditMode.Move);
            editKeyMap.Add(Keys.W, EditMode.Pan);
            editKeyMap.Add(Keys.E, EditMode.Zoom);

            editKeyMap.Add(Keys.F, EditMode.FrameAll);
            editKeyMap.Add(Keys.Delete, EditMode.Delete);

            Cursor selectCursor =
                NativeMethods.LoadCursor(CurveControlResources.SelectCursor);

            cursorMap.Add(EditMode.None, Cursors.Default);
            cursorMap.Add(EditMode.Add,
                NativeMethods.LoadCursor(CurveControlResources.AddCursor));
            cursorMap.Add(EditMode.Pan,
                NativeMethods.LoadCursor(CurveControlResources.PanCursor));
            cursorMap.Add(EditMode.Zoom,
                NativeMethods.LoadCursor(CurveControlResources.ZoomCursor));
            cursorMap.Add(EditMode.Select, selectCursor);
            cursorMap.Add(EditMode.Move,
                NativeMethods.LoadCursor(CurveControlResources.MoveCursor));

            SelectCursorShape();
        }
        #endregion

        #region Public Methods
        public void BeginUpdate()
        {
            batchUpdating = true;
            BeginCaptureCommands();
            BeginUpdateCurves();
        }

        public void EndUpdate()
        {
            EndUpdateCurves();
            EndCaptureCommands();
            batchUpdating = false;
            if (renderRequested) RequestRender();
        }

        public void FrameAll()
        {
            FrameSelection(true);
        }
        #endregion

        #region Event Handling
        private void curveView_Paint(object sender, PaintEventArgs e)
        {
            DrawAll(e.Graphics);
        }

        private void curveView_KeyDown(object sender, KeyEventArgs e)
        {
            if (quickEditMode == EditMode.None && mousePressing == false )
            {
                EditMode em;
                if (quickEditKeyMap.TryGetValue(e.KeyCode, out em))
                    quickEditMode = em;
                else if (editKeyMap.TryGetValue(e.KeyCode, out em))
                    SetEditMode(em);

                SelectCursorShape();
            }
        }

        private void curveView_KeyUp(object sender, KeyEventArgs e)
        {
            if (quickEditMode != EditMode.None)
            {
                EditMode em;
                if (quickEditKeyMap.TryGetValue(e.KeyCode, out em) &&
                    em == quickEditMode)
                {
                    quickEditMode = EditMode.None;
                    mousePressing = false;
                }

                SelectCursorShape();
            }
        }

        private void curveView_MouseDown(object sender, MouseEventArgs e)
        {
            if ( ( e.Button & MouseButtons.Left ) != MouseButtons.Left ) return;
            if (mousePressing) return;

            mousePressing = true;

            clickedPos = prevMousePos = new Vector2(e.X, e.Y);

            EditMode em = ( quickEditMode != EditMode.None )? quickEditMode: editMode;

            if (editable)
            {
                switch (em)
                {
                    case EditMode.Select:
                        selectingBox = new RectangleF(e.X, e.Y, 0, 0);
                        break;
                    case EditMode.Move:
                        BeginUpdateCurves();
                        break;
                }
            }
        }

        private void curveView_MouseMove(object sender, MouseEventArgs e)
        {
            EditMode em = (quickEditMode != EditMode.None) ? quickEditMode : editMode;
            if (em == EditMode.None) return;
            if (!mousePressing) return;

            SelectCursorShape();

            Vector2 pos_ps = new Vector2(e.X, e.Y);
            if ((e.Button & MouseButtons.Left)== MouseButtons.Left)
            {
                switch (em)
                {
                    case EditMode.Pan:
                        curveView.Pan(pos_ps - prevMousePos);
                        break;
                    case EditMode.Zoom:
                        curveView.Zoom(pos_ps - prevMousePos);
                        break;
                    case EditMode.Select:
                        if (editable)
                        {
                            Vector2 min = Vector2.Min(clickedPos, pos_ps);
                            Vector2 max = Vector2.Max(clickedPos, pos_ps);
                            selectingBox = new RectangleF(min.X, min.Y,
                                max.X - min.X, max.Y - min.Y);
                        }
                        break;
                    case EditMode.Move:
                        if (editable)
                        {
                            Vector2 pos_us = curveView.ToUnitCoordinate(pos_ps);
                            MathHelper.Clamp(pos_us.X, 
                                -1f * MaximumCurveKeyEntry,
                                MaximumCurveKeyEntry);
                            MathHelper.Clamp(pos_us.Y, 
                                -1f * MaximumCurveKeyEntry,
                                MaximumCurveKeyEntry);

                            Vector2 prevPos_us =
                                curveView.ToUnitCoordinate(prevMousePos);
                            foreach (EditCurve curve in curves)
                                curve.Move(pos_us, prevPos_us);

                            CheckSelection();
                            if (autoFrame) FrameSelection(true);
                        }
                        break;
                }

                prevMousePos = pos_ps;
                Invalidate(true);
            }

        }

        private void curveView_MouseUp(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != MouseButtons.Left) return;
            if (!mousePressing) return;

            mousePressing = false;

            // Quit if edtable is false.
            if (!editable) return;

            EditMode em = (quickEditMode != EditMode.None) ? quickEditMode : editMode;
            switch (em)
            {
                case EditMode.Add:
                    Vector2 pos_us = curveView.ToUnitCoordinate( prevMousePos );
                    MathHelper.Clamp(pos_us.X,
                        -1f * MaximumCurveKeyEntry,
                        MaximumCurveKeyEntry);
                    MathHelper.Clamp(pos_us.Y,
                        -1f * MaximumCurveKeyEntry,
                        MaximumCurveKeyEntry); 

                    UpdateCurves(delegate(EditCurve curve)
                    {
                        curve.AddKey( pos_us );
                    } );
                    break;
                case EditMode.Move:

                    // Finish move curves.
                    BeginCaptureCommands();
                    EndUpdateCurves();
                    EndCaptureCommands();
                    RequestRender();

                    break;
                case EditMode.Select:
                    bool singleSelect = false;

                    // adjust selecting box if select area is too small.
                    if (selectingBox.Width < MinSelectSize ||
                        selectingBox.Height < MinSelectSize)
                    {
                        selectingBox.X += (selectingBox.Width - MinSelectSize) * 0.5f;
                        selectingBox.Y += (selectingBox.Height- MinSelectSize) * 0.5f;
                        selectingBox.Width = selectingBox.Height = MinSelectSize;
                        singleSelect = true;
                    }

                    BoundingBox selectRegion = new BoundingBox(
                        new Vector3(curveView.ToUnitCoordinate(selectingBox.Left,
                                        selectingBox.Bottom), -1),
                        new Vector3(curveView.ToUnitCoordinate(selectingBox.Right,
                                        selectingBox.Top), 1));

                    Vector2 tangentScale = new Vector2(
                        (float)curveView.ScaleX * TangentManipulatorLength,
                        (float)curveView.ScaleY * TangentManipulatorLength);

                    // Check selection front to back.
                    bool selected = false;
                    int curveCount = curves.Count;
                    while (curveCount-- > 0)
                    {
                        EditCurve curve = curves[curveCount];

                        if (!curve.Editable || !curve.Visible) continue;

                        if (singleSelect && selected)
                        {
                            curve.ClearSelection();
                        }
                        else
                        {
                            curve.Select(selectRegion, tangentScale, curveKeyViewMode,
                                    curveTangentsViewMode, ModifierKeys == Keys.Shift,
                                    singleSelect);

                            selected = (singleSelect && curve.Selection.Count != 0);
                        }
                    }

                    selectingBox = RectangleF.Empty;
                    CheckSelection();
                    RequestRender();
                    break;
            }
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            ToolStripButton button = (ToolStripButton)sender;
            SetEditMode((EditMode)Enum.Parse(typeof(EditMode), button.Tag.ToString()));
        }

        private void undoRedoToolStripMenuItems_Click(object sender, EventArgs e)
        {
            if (sender == undoToolStripMenuItem)
                commandHistory.Undo();
            else
                commandHistory.Redo();

            CheckSelection();
            RequestRender();
        }

        private void frameAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrameSelection(true);
        }

        private void frameSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrameSelection(false);
        }

        private void autoFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoFrame = !autoFrame;
            autoFrameToolStripMenuItem.Checked = autoFrame;
        }

        private void curveSmoothnessMenuItem_Click(object sender, EventArgs e)
        {
            smoothness = (CurveSmoothness)Enum.Parse(
                typeof(CurveSmoothness), CheckMenuItem(sender));
            RequestRender();
        }

        private void infinityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showInfinitCurve = !showInfinitCurve;
            ((ToolStripMenuItem)sender).Checked = showInfinitCurve;
            RequestRender();
        }

        private void keyViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            curveKeyViewMode = (EditCurveView)Enum.Parse(
                typeof(EditCurveView), CheckMenuItem(sender));
            RequestRender();
        }

        private void tangentViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            curveTangentsViewMode = (EditCurveView)Enum.Parse(
                typeof(EditCurveView), CheckMenuItem(sender));
            RequestRender();
        }

        private static void CheckMenuItem(ToolStripMenuItem parent, string name)
        {
            char[] splitChars = { ',' };
            foreach (object o in parent.DropDownItems)
            {
                ToolStripMenuItem item = o as ToolStripMenuItem;
                if (item != null && item.Tag != null)
                {
                    string[] texts = item.Tag.ToString().Split(splitChars);
                    item.Checked =
                        String.Compare(texts[texts.Length - 1], name, true) == 0;
                }
            }
        }

        private void curveLoopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;

            // Set curve loopType of curves.
            // It assumes sender menuItem has a string like
            // "[Pre|Post], CurveLoopType".
            string[] texts = item.Tag.ToString().Split(new char[] { ',' });
            bool preLoop = String.Compare("pre", texts[0], true) == 0;
            CurveLoopType loopType = (CurveLoopType)Enum.Parse(
                                        typeof(CurveLoopType), texts[1]);

            UpdateCurves(delegate(EditCurve curve)
            {
                if (preLoop)
                    curve.PreLoop = loopType;
                else
                    curve.PostLoop = loopType;
            });
        }

        private void tangentsToolStripMenuItems_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;

            // Set tangent type of curves.
            // It assumes sender menuItem has a string like
            // "EditCurveSelections, EditCurveTangent".
            string[] texts = item.Tag.ToString().Split(new char[] { ',' });
            EditCurveSelections selection = (EditCurveSelections)Enum.Parse(
                                                typeof(EditCurveSelections), texts[0]);

            EditCurveTangent tangent = (EditCurveTangent)Enum.Parse(
                                            typeof(EditCurveTangent), texts[1]);

            UpdateCurves(delegate(EditCurve curve)
            {
                curve.SetTangents(selection, tangent);
            });
        }

        private void singleKeyEdit_TextChanged(object sender, EventArgs e)
        {
            if (singleEditUpdating) return;

            float newPosition;
            if (!Single.TryParse(keyPositionTextBox.Text, out newPosition))
                newPosition = singleEditKey.Position;
            if (newPosition > MaximumCurveKeyEntry * (float)curveView.ScaleX)
            {
                newPosition = MaximumCurveKeyEntry * (float)curveView.ScaleX;
                keyPositionTextBox.Text = newPosition.ToString();
            }
            else if (newPosition < -1f * MaximumCurveKeyEntry * (float)curveView.ScaleX)
            {
                newPosition = -1f * MaximumCurveKeyEntry * (float)curveView.ScaleX;
                keyPositionTextBox.Text = newPosition.ToString();
            }

            float newValue;
            if (!Single.TryParse(keyValueTextBox.Text, out newValue))
                newValue = singleEditKey.Value;
            if (newValue > MaximumCurveKeyEntry * (float)curveView.ScaleY)
            {
                newValue = MaximumCurveKeyEntry * (float)curveView.ScaleY;
                keyValueTextBox.Text = newValue.ToString();
            }
            else if (newValue < -1f * MaximumCurveKeyEntry * (float)curveView.ScaleY)
            {
                newValue = -1f * MaximumCurveKeyEntry * (float)curveView.ScaleY;
                keyValueTextBox.Text = newValue.ToString();
            }

            BeginCaptureCommands();
            BeginUpdateCurves();
            singleEditCurve.UpdateKey(singleEditKey.Id, newPosition, newValue);
            EndUpdateCurves();
            EndCaptureCommands();

            if (autoFrame) FrameSelection(true);

            RequestRender();
        }

        private void CurveControl_Load(object sender, EventArgs e)
        {
            ISite site = (ParentForm!=null)? ParentForm.Site: null;
            commandHistory = CommandHistory.EnsureHasService(site);
            commandHistory.Changed += new EventHandler(commandHistory_Changed);
        }

        void commandHistory_Changed(object sender, EventArgs e)
        {
            CommandHistory cmd = sender as CommandHistory;

            // Update Undo/Redo menu enables.
            undoToolStripMenuItem.Enabled = cmd.CanUndo;
            redoToolStripMenuItem.Enabled = cmd.CanRedo;
        }

        void curves_AddingCurve(object sender, EditCurveEventArgs e)
        {
            e.Curve.StateChanged += new EventHandler(Curve_StateChanged);
        }

        void curves_RemovingCurve(object sender, EditCurveEventArgs e)
        {
            e.Curve.StateChanged -= new EventHandler(Curve_StateChanged);
        }

        void curves_Changed(object sender, EventArgs e)
        {
            RequestRender();
        }

        void Curve_StateChanged(object sender, EventArgs e)
        {
            RequestRender();
        }
        #endregion

        #region Render
        /// <summary>
        /// Draw Line with clipping.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="pen"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        private void DrawLine(Graphics g, Pen pen, Vector2 p1, Vector2 p2)
        {
            float cw = curveView.ClientSize.Width, ch = curveView.ClientSize.Height;
            if ( ( p1.X < 0.0f && p2.X < 0.0f ) || ( p1.Y < 0.0f && p2.Y < 0.0f ) ||
                ( p1.X > cw && p2.X > cw ) || ( p1.Y > ch && p2.Y > ch ) ) return;

            g.DrawLine( pen, p1.X, p1.Y, p2.X, p2.Y);
        }

        /// <summary>
        /// Fill Rectangle with clipping.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        private void FillRectangle(Graphics g, Brush brush, float x, float y,
                                    float w, float h)
        {
            float cw = curveView.ClientSize.Width, ch = curveView.ClientSize.Height;
            if ( x + w < 0.0f || y + h < 0.0f || x > cw || y > ch ) return;
            g.FillRectangle(brush, x, y, w, h);
        }

        private void DrawArrow(Graphics g, Vector2 org, float angle, float length,
                                        System.Drawing.Color col)
        {
            Pen pen = new Pen(col);
            Brush brush = new SolidBrush(col);

            float s = (float)Math.Sin(angle), c = (float)Math.Cos(angle);

            Vector2 pt = new Vector2(org.X + (c * length), org.Y - (s * length));
            DrawLine(g, pen, org, pt);

            // Draw arrow shape polyg if it inside curveView client.
            const float arrowSize = 10.0f;
            if (pt.X > -arrowSize &&
                pt.X < (float)curveView.ClientSize.Width + arrowSize ||
                pt.Y > -arrowSize &&
                pt.Y < (float)curveView.ClientSize.Height + arrowSize)
            {
                // transform points.
                for (int i = 0; i < arrowPositions.Length; ++i)
                {
                    transformedArrowPositions[i].X =
                        pt.X + (c * arrowPositions[i].X - s * arrowPositions[i].Y);
                    transformedArrowPositions[i].Y =
                        pt.Y - (s * arrowPositions[i].X + c * arrowPositions[i].Y);
                }

                g.FillPolygon(brush, transformedArrowPositions);
            }
        }

        private void DrawTangents(Graphics g, EditCurve curve)
        {
            System.Drawing.Color[] colors = new System.Drawing.Color[] { System.Drawing.Color.Yellow, System.Drawing.Color.Brown };
            float len = 50.0f;
            Vector2 pt1 = new Vector2();

            EditCurveSelections tangentSelection = EditCurveSelections.Key |
                                                    EditCurveSelections.TangentIn |
                                                    EditCurveSelections.TangentOut;

            for (int i = 0; i < curve.Keys.Count; ++i)
            {
                EditCurveKey editKey = curve.Keys[i];
                CurveKey key = editKey.OriginalKey;

                EditCurveSelections selection = editKey.Selection;
                bool isSelecting = (selection & tangentSelection) != 0;

                if (((isSelecting && curve.Editable) ||
                    curveTangentsViewMode == EditCurveView.Always)
                    && key.Continuity == CurveContinuity.Smooth)
                {
                    pt1 = curveView.ToPixelCoordinate(key.Position, key.Value);

                    int colIdx = 0;
                    float screen_tangent;

                    // tan-in.
                    colIdx = (selection & EditCurveSelections.TangentIn) != 0 ? 0 : 1;
                    screen_tangent = curveView.UnitToScreenTangentAngle(
                            key.TangentIn / curve.GetDistanceOfKeys(i, 0));

                    DrawArrow(g, pt1,
                        (float)Math.PI + (float)Math.Atan(screen_tangent),
                        len, colors[colIdx]);

                    // tan-out.
                    colIdx = (selection & EditCurveSelections.TangentOut) != 0 ? 0 : 1;
                    screen_tangent = curveView.UnitToScreenTangentAngle(
                        key.TangentOut / curve.GetDistanceOfKeys(i, 1));

                    DrawArrow(g, pt1, (float)Math.Atan(screen_tangent), len,
                        colors[colIdx]);
                }
            }
        }

        private void DrawTangents(Graphics g)
        {
            foreach (EditCurve curve in curves)
            {
                if (curve.Visible)
                    DrawTangents(g, curve);
            }
        }

        private void DrawCurveKeys(Graphics g)
        {
            Brush selected_brush = new SolidBrush(System.Drawing.Color.Yellow);
            Brush unselected_brush = new SolidBrush(System.Drawing.Color.Black);

            int s = 4;
            int hs = 2;
            Vector2 pt = new Vector2();

            // first, draw unselected keyframes.
            if (curveKeyViewMode == EditCurveView.Always)
            {
                foreach (EditCurve curve in curves)
                {
                    if (!curve.Visible) continue;

                    foreach (EditCurveKey key in curve.Keys)
                    {
                        if ((key.Selection & EditCurveSelections.Key) == 0 ||
                            curve.Editable == false)
                        {
                            pt = curveView.ToPixelCoordinate(key.Position, key.Value);

                            FillRectangle(g, unselected_brush, pt.X - hs, pt.Y - hs,
                                s, s);
                        }
                    }
                }
            }

            // Then draw selected keyframes. Thus user can see selected keyframe when
            // there are unselected key at same position.
            foreach (EditCurve curve in curves)
            {
                if (!curve.Editable || !curve.Visible)
                    continue;

                foreach (EditCurveKey key in curve.Keys)
                {
                    if ((key.Selection & EditCurveSelections.Key) != 0)
                    {
                        pt = curveView.ToPixelCoordinate(key.Position, key.Value);
                        FillRectangle(g, selected_brush, pt.X - hs, pt.Y - hs, s, s);
                    }
                }
            }
        }

        private void DrawCurveSections(Graphics g, Pen pen, EditCurve curve,
                                            double t0, double t1, double step)
        {
            Vector2[] p = new Vector2[2] { new Vector2(), new Vector2() };

            int pIdx = 0;
            p[pIdx] = 
                curveView.ToPixelCoordinate((float)t0, curve.Evaluate((float)t0));

            for (double t = t0; t < t1; )
            {
                double nextT = t + step;
                t = nextT;
                pIdx = (pIdx + 1) & 1;
                p[pIdx] =
                    curveView.ToPixelCoordinate((float)t, curve.Evaluate((float)t));

                DrawLine(g, pen, p[0], p[1]);
                t = nextT;
            }
        }


        /// <summary>
        /// Draw actual part of curve.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="pen"></param>
        /// <param name="curve"></param>
        /// <param name="t0"></param>
        /// <param name="t1"></param>
        /// <param name="step"></param>
        private void DrawCurve(Graphics g, Pen pen, EditCurve curve,
                                    double t0, double t1, double step)
        {
            Vector2[] p = new Vector2[2] { new Vector2(), new Vector2() };

            // Search key and next key that includes t0 position.
            int keyIndex = 0;
            EditCurveKey key = null, nextKey = null;
            for (; keyIndex < curve.Keys.Count; ++keyIndex)
            {
                key = nextKey; nextKey = curve.Keys[keyIndex];
                if (nextKey.Position > t0) break;
            }

            int pIdx = 0;
            p[pIdx] = curveView.ToPixelCoordinate((float)t0, curve.Evaluate((float)t0));
            for (double t = t0; t < t1;)
            {
                double nextT = t1 + step;
                if (nextKey != null)
                    nextT = Math.Min(t1, nextKey.Position);

                // Draw current key and next key section.
                if (key.Continuity == CurveContinuity.Smooth)
                {
                    while (t < nextT)
                    {
                        // If this line crosses next key position, draw line from
                        // current position to next key position.
                        t = ( t < nextT && t + step > nextT )? nextT: t + step;
                        pIdx = (pIdx + 1) & 1;
                        p[pIdx] = curveView.ToPixelCoordinate(
                                                    (float)t, curve.Evaluate((float)t));
                        DrawLine(g, pen, p[0], p[1]);
                    }
                }
                else
                {
                    // Step case,
                    // Draw, horizontal line.
                    pIdx = (pIdx + 1) & 1;
                    p[pIdx] = curveView.ToPixelCoordinate(nextKey.Position, key.Value);
                    DrawLine(g, pen, p[0], p[1]);

                    // Draw vertical line.
                    pIdx = (pIdx + 1) & 1;
                    p[pIdx] = curveView.ToPixelCoordinate(
                                                    nextKey.Position, nextKey.Value);
                    DrawLine(g, pen, p[0], p[1]);

                    t = nextT;
                }

                // Advance to next key.
                key = nextKey;
                nextKey = (++keyIndex < curve.Keys.Count) ? curve.Keys[keyIndex] : null;
            }
        }

        private void DrawCurves(Graphics g)
        {
            double dt0 = curveView.OffsetX;
            double dt1 = dt0 + (double)curveView.ViewSize.X * curveView.ScaleX;
            double step = curveView.ScaleX * smoothnessTable[(int)smoothness];

            foreach (EditCurve curve in curves)
            {
                if (!curve.Visible) continue;

                System.Drawing.Color curveColor = curve.Color;

                Vector3 c = Vector3.Lerp(
                    new Vector3(curveColor.R, curveColor.G, curveColor.B),
                    new Vector3(BackColor.R, BackColor.G, BackColor.B),
                    0.75f);

                System.Drawing.Color infinityCurveColor = System.Drawing.Color.FromArgb((int)c.X, (int)c.Y, (int)c.Z);

                Pen pen0 = new Pen(curveColor, CurvePenWidth);
                Pen pen1 = new Pen(infinityCurveColor, CurvePenWidth);

                if (curve.Keys.Count > 0)
                {
                    double kt0 = curve.Keys[0].Position;
                    double kt1 = curve.Keys[curve.Keys.Count - 1].Position;

                    double t0 = dt0;
                    double t1 = Math.Min(dt1, kt0);

                    // draw pre-section
                    if (t0 < t1 && showInfinitCurve)
                    {
                        if (curve.Keys.Count == 1 ||
                            curve.PreLoop == CurveLoopType.Constant ||
                            curve.PreLoop == CurveLoopType.Linear)
                        {
                            // draw straight line.
                            Vector2 p0 = curveView.ToPixelCoordinate(
                                            (float)t0, curve.Evaluate((float)t0));
                            Vector2 p1 = curveView.ToPixelCoordinate(
                                            (float)t1, curve.Evaluate((float)t1));
                            DrawLine(g, pen1, p0, p1);
                        }
                        else
                        {
                            DrawCurveSections(g, pen1, curve, t0, t1, step);
                        }
                    }

                    // draw fact section
                    t0 = Math.Max(dt0, kt0);
                    t1 = Math.Min(dt1, kt1);
                    if (t0 < t1)
                        DrawCurve(g, pen0, curve, t0, t1, step);

                    // draw post-section
                    t0 = Math.Min(dt1, kt1);
                    t1 = dt1;
                    if (t0 < t1 && showInfinitCurve)
                    {
                        if (curve.Keys.Count == 1 ||
                            curve.PostLoop == CurveLoopType.Constant ||
                            curve.PostLoop == CurveLoopType.Linear)
                        {
                            // draw straight line.
                            Vector2 p0 = curveView.ToPixelCoordinate(
                                                (float)t0, curve.Evaluate((float)t0));
                            Vector2 p1 = curveView.ToPixelCoordinate(
                                                (float)t1, curve.Evaluate((float)t1));
                            DrawLine(g, pen1, p0, p1);
                        }
                        else
                        {
                            DrawCurveSections(g, pen1, curve, t0, t1, step);
                        }
                    }
                }
            }
        }

        protected void DrawAll(Graphics g)
        {
            curveView.DrawGridLines(g);
            DrawCurves(g);

            if (curveTangentsViewMode != EditCurveView.Never)
                DrawTangents(g);

            if (curveKeyViewMode != EditCurveView.Never)
                DrawCurveKeys(g);

            // Draw Selection box
            if (selectingBox != RectangleF.Empty)
            {
                g.FillRectangle(new SolidBrush(SelectingBoxColor), selectingBox);
                g.DrawRectangle(new Pen(SelectingBoxBorderColor),
                                    System.Drawing.Rectangle.Truncate(selectingBox));
            }
        }

        #endregion

        #region Private methods

        private void SelectCursorShape()
        {
            EditMode em = (quickEditMode != EditMode.None) ? quickEditMode : editMode;
            Cursor cur;
            if (cursorMap.TryGetValue(em, out cur))
                curveView.Cursor = cur;
        }

        private void BeginCaptureCommands()
        {
            commandHistory.BeginRecordCommands();
        }

        private void EndCaptureCommands()
        {
            commandHistory.EndRecordCommands();
        }

        private void BeginUpdateCurves()
        {
            // Ensure current update is finished.
            if (updatingCurve)
                EndUpdateCurves();

            updatingCurve = true;
            foreach (EditCurve curve in curves)
                curve.BeginUpdate();
        }

        private void EndUpdateCurves()
        {
            foreach (EditCurve curve in curves)
                curve.EndUpdate();
            updatingCurve = false;
        }

        private void SetEditMode(EditMode newMode)
        {
            bool processed = true;
            switch (newMode)
            {
                case EditMode.Delete:
                    UpdateCurves(delegate(EditCurve curve) { curve.RemoveKeys(); });
                    break;
                case EditMode.FrameAll:
                    FrameSelection(true);
                    break;
                default:
                    processed = false;
                    break;
            }

            if (!processed)
            {
                editMode = newMode;

                // Update edit mode buttons.
                foreach (ToolStripButton editButton in editButtons)
                    editButton.Checked = ((EditMode)Enum.Parse(
                        typeof(EditMode), editButton.Tag.ToString()) == editMode);

                SelectCursorShape();
            }
        }

        private void FrameSelection(bool allKeys)
        {
            // Get min max value of all curves and keys.
            Vector2 min = new Vector2(Single.MaxValue, Single.MaxValue);
            Vector2 max = new Vector2(Single.MinValue, Single.MinValue);

            bool needToDefault = true;
            foreach ( EditCurve curve in curves)
            {
                if (!allKeys && curve.Visible == false) continue;

                for (int idx = 0; idx < curve.Keys.Count; ++idx)
                {
                    EditCurveKey key = curve.Keys[idx];

                    if (!allKeys && key.Selection == EditCurveSelections.None) continue;

                    Vector2 pos = new Vector2(key.Position, key.Value);
                    min = Vector2.Min(min, pos);
                    max = Vector2.Max(max, pos);

                    // Sample few point for more aculate result.
                    if (idx < curve.Keys.Count - 1)
                    {
                        float nextPosition = curve.Keys[idx+1].Position;

                        const int sampleCount = 4;
                        // at 0 is already sampled and 1 will be sampled in next loop.
                        float dt = 1.0f / (float)(sampleCount + 2);
                        float nt = dt;
                        for (int i = 0; i < sampleCount; ++i, nt += dt)
                        {
                            float t = MathHelper.Lerp( key.Position, nextPosition, nt );
                            pos = new Vector2( t, curve.Evaluate( t ) );
                            min = Vector2.Min(min, pos);
                            max = Vector2.Max(max, pos);
                        }
                    }
                    needToDefault = false;
                }
            }

            if ( needToDefault )
            {
                min = -Vector2.One;
                max = Vector2.One;
            }

            if ( curveView.Frame(min, max) )
                RequestRender();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (mousePressing == false)
            {
                curveView.Zoom(e.Delta);
                RequestRender();
            }

            base.OnMouseWheel(e);
        }

        delegate void UpdateCurveDelegate(EditCurve curve);

        /// <summary>
        /// Update curves.
        /// </summary>
        /// <param name="callback"></param>
        private void UpdateCurves(UpdateCurveDelegate callback)
        {
            if (!editable) return;

            BeginCaptureCommands();
            BeginUpdateCurves();
            foreach (EditCurve curve in curves)
            {
                if ( curve.Editable ) callback(curve);
            }
            EndUpdateCurves();
            EndCaptureCommands();
            CheckSelection();
            RequestRender();
        }

        protected void RequestRender()
        {
            if ( batchUpdating == false )
            {
                Invalidate(true);
                renderRequested = false;
            }
            else
            {
                renderRequested = true;
            }
        }

        private void CheckSelection()
        {
            // Make sure it selected single key.
            CommonState<CurveLoopType> preLoop = new CommonState<CurveLoopType>();
            CommonState<CurveLoopType> postLoop = new CommonState<CurveLoopType>();
            CommonState<EditCurveTangent> tangentInType =
                new CommonState<EditCurveTangent>();
            CommonState<EditCurveTangent> tangentOutType =
                new CommonState<EditCurveTangent>();

            int totalSelectCount = 0;
            EditCurve lastSelectedCurve = null;
            foreach (EditCurve curve in curves)
            {
                if (!curve.Editable || !curve.Visible ||
                        curve.Selection.Count == 0 ) continue;

                EditCurveKeySelection selection = curve.Selection;
                totalSelectCount += selection.Count;
                lastSelectedCurve = curve;

                preLoop.Add(curve.PreLoop);
                postLoop.Add(curve.PostLoop);

                foreach (long id in selection.Keys)
                {
                    EditCurveKey key;
                    if (curve.Keys.TryGetValue(id, out key))
                    {
                        if (key.Continuity == CurveContinuity.Smooth)
                        {
                            tangentInType.Add(key.TangentInType);
                            tangentOutType.Add(key.TangentOutType);
                        }
                        else
                        {
                            tangentInType.Add(EditCurveTangent.Stepped);
                            tangentOutType.Add(EditCurveTangent.Stepped);
                        }
                    }
                }
            }

            // Update Menus.
            CheckMenuItem(preInfinityToolStripMenuItem, preLoop.ValueString);
            CheckMenuItem(postInfinityToolStripMenuItem, postLoop.ValueString);

            bool sameValueString = String.Compare(tangentInType.ValueString,
                tangentOutType.ValueString) == 0;
            string tangentsString = sameValueString?
                        tangentInType.ValueString : String.Empty;

            CheckMenuItem(tangentsToolStripMenuItem, tangentsString);
            CheckMenuItem(inTangentToolStripMenuItem, tangentInType.ValueString);
            CheckMenuItem(outTangentToolStripMenuItem, tangentOutType.ValueString);

            // Are we just select one key?
            singleEditUpdating = true;
            if (totalSelectCount == 1)
            {
                singleEditCurve = lastSelectedCurve;

                EditCurveKey[] keys = lastSelectedCurve.GetSelectedKeys();
                singleEditKey = keys[0];
                keyPositionTextBox.Text =
                    String.Format( "{0:F3}", singleEditKey.Position );

                keyValueTextBox.Text = String.Format("{0:F3}", singleEditKey.Value);

                keyPositionLabel.Enabled = keyValueLabel.Enabled =
                    keyPositionTextBox.Enabled = keyValueTextBox.Enabled = true;
            }
            else
            {
                singleEditKey = null;
                singleEditCurve = null;
                keyPositionTextBox.Text = keyValueTextBox.Text = String.Empty;
                keyPositionLabel.Enabled = keyValueLabel.Enabled =
                    keyPositionTextBox.Enabled = keyValueTextBox.Enabled = false;
            }
            singleEditUpdating = false;
        }

        /// <summary>
        /// Selecte given item and uncheck other siblings.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        private static string CheckMenuItem(object sender)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            ToolStripMenuItem parent = item.OwnerItem as ToolStripMenuItem;
            foreach (ToolStripMenuItem child in parent.DropDownItems)
                child.Checked = (child == item);

            return item.Tag.ToString();
        }
        #endregion

        #region Properties wrapper members
        private System.Drawing.Color selectingBoxColor = System.Drawing.Color.FromArgb(80, 0, 200, 128);
        private System.Drawing.Color selectingBoxBorderColor = System.Drawing.Color.Black;
        #endregion

        #region Private Members
        enum EditMode
        {
            None,
            Pan,
            Zoom,
            Select,
            Move,
            Add,
            Delete,
            FrameAll
        };

        private CommandHistory commandHistory;
        private EditCurveCollection curves;

        // Curve display controls.
        private int[]           smoothnessTable         = new int[] { 64, 32, 8, 4 };
        private EditCurveView   curveKeyViewMode        = EditCurveView.Always;
        private EditCurveView   curveTangentsViewMode   = EditCurveView.OnActiveKeys;

        // Show out of range part of curve.
        private bool            showInfinitCurve        = true;

        private CurveSmoothness smoothness              = CurveSmoothness.Fine;

        // For UI
        private EditMode    editMode        = EditMode.Select;
        private EditMode    quickEditMode   = EditMode.None;

        // Mouse clicked position in unit coordinate.
        private Vector2     clickedPos      = new Vector2();
        private Vector2     prevMousePos    = new Vector2();
        private RectangleF  selectingBox    = new RectangleF();

        // Control doesn't render when this flag is true.
        private bool        batchUpdating   = false;

        // Render requres occured while batch updating.
        private bool        renderRequested = false;
        private bool        autoFrame       = false;
        private bool        mousePressing   = false;

        private bool singleEditUpdating;

        private Dictionary<Keys, EditMode> quickEditKeyMap;
        private Dictionary<Keys, EditMode> editKeyMap;
        private Dictionary<EditMode, Cursor> cursorMap =
            new Dictionary<EditMode,Cursor>();

        private bool updatingCurve;
        private bool editable = true;

        private ToolStripButton[] editButtons;

        private EditCurve singleEditCurve;
        private EditCurveKey singleEditKey;

        // Arrow shape information.
        static protected PointF[] arrowPositions = new PointF[]
        {
           new PointF(  4.0f, 0.0f ),
           new PointF(  0.0f, 4.0f ),
           new PointF(  0.0f,-4.0f ),
        };

        static protected PointF[] transformedArrowPositions =
            new PointF[] { new PointF(), new PointF(), new PointF() };


        #endregion
    }
}
