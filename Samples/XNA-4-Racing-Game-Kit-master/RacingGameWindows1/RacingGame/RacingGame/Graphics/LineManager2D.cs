#region File Description
//-----------------------------------------------------------------------------
// LineManager2D.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Unit Testing
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RacingGame.Graphics;
using RacingGame.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using RacingGame.Shaders;
#endregion

namespace RacingGame.Graphics
{
    /// <summary>
    /// LineManager class, used for drawing lines. We can't simply draw
    /// lines like in OpenGL (glLine) because we need to do everything with
    /// vertex buffers, which is a bit more work (and using DrawPrimitives
    /// directly produces way worse code).
    /// </summary>
    public class LineManager2D : IDisposable
    {
        #region Variables
        /// <summary>
        /// Number of lines used this frame, will be set to 0 when rendering.
        /// </summary>
        private int numOfLines = 0;

        /// <summary>
        /// The actual list for all the lines, it will NOT be reseted each
        /// frame like numOfLines! We will remember the last lines and
        /// only change this list when anything changes (new line, old
        /// line missing, changing line data).
        /// When this happens buildVertexBuffer will be set to true.
        /// </summary>
        private List<Line> lines = new List<Line>();

        /// <summary>
        /// Struct for a line, instances of this struct will be added to lines.
        /// </summary>
        struct Line
        {
            #region Variables
            /// <summary>
            /// Positions
            /// </summary>
            public Point startPoint, endPoint;
            /// <summary>
            /// Color
            /// </summary>
            public Color color;
            #endregion

            #region Constructor
            /// <summary>
            /// Create line
            /// </summary>
            /// <param name="setStartPoint">Set start point</param>
            /// <param name="setEndPoint">Set end point</param>
            /// <param name="setColor">Set color</param>
            public Line(Point setStartPoint,
                Point setEndPoint, Color setColor)
            {
                startPoint = setStartPoint;
                endPoint = setEndPoint;
                color = setColor;
            }
            #endregion

            #region Equals operators
            /// <summary>
            /// Are these two Lines equal?
            /// </summary>
            public static bool operator ==(Line a, Line b)
            {
                return a.startPoint == b.startPoint &&
                    a.endPoint == b.endPoint &&
                    a.color == b.color;
            }

            /// <summary>
            /// Are these two Lines not equal?
            /// </summary>
            public static bool operator !=(Line a, Line b)
            {
                return a.startPoint != b.startPoint ||
                    a.endPoint != b.endPoint ||
                    a.color != b.color;
            }

            /// <summary>
            /// Support Equals(.) to keep the compiler happy
            /// (because we used == and !=)
            /// </summary>
            public override bool Equals(object a)
            {
                if (a is Line)
                    return (Line)a == this;

                return false;
            }

            /// <summary>
            /// Support GetHashCode() to keep the compiler happy
            /// (because we used == and !=)
            /// </summary>
            public override int GetHashCode()
            {
                return 0; // Not supported or nessescary
            }
            #endregion
        }

        /// <summary>
        /// Build vertex buffer this frame because the line list was changed?
        /// Note: The vertex buffer implementation was removed some time ago,
        /// but this variable is still used to check for updates to lineVertices!
        /// </summary>
        private bool buildVertexBuffer = false;

        /// <summary>
        /// Vertex buffer for all lines
        /// </summary>
        VertexPositionColor[] lineVertices =
            new VertexPositionColor[MaxNumOfLines * 2];
        /// <summary>
        /// Real number of primitives currently used.
        /// </summary>
        private int numOfPrimitives = 0;

        /// <summary>
        /// Max. number of lines allowed.
        /// </summary>
        private const int MaxNumOfLines = 64;

        #endregion

        #region Constructor
        /// <summary>
        /// Create LineManager
        /// </summary>
        public LineManager2D()
        {
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing">Disposing</param>
        protected virtual void Dispose(bool disposing)
        {
        }
        #endregion

        #region Vertex buffer stuff
        /// <summary>
        /// Update vertex buffer
        /// </summary>
        private void UpdateVertexBuffer()
        {
            // Don't do anything if we got no lines.
            if (numOfLines == 0 ||
                // Or if some data is invalid
                lines.Count < numOfLines)
            {
                numOfPrimitives = 0;
                return;
            }
#if LOG_STUFF
            Log.Write("LineManager.UpdateVertexBuffer() numOfLines=" +
                numOfLines + ", buildVertexBuffer=" + buildVertexBuffer);
#endif

            // Set all lines
            for (int lineNum = 0; lineNum < numOfLines; lineNum++)
            {
                Line line = (Line)lines[lineNum];
                lineVertices[lineNum * 2 + 0] = new VertexPositionColor(
                    new Vector3(
                    -1.0f + 2.0f * line.startPoint.X / BaseGame.Width,
                    -(-1.0f + 2.0f * line.startPoint.Y / BaseGame.Height), 0),
                    line.color);
                lineVertices[lineNum * 2 + 1] = new VertexPositionColor(
                    new Vector3(
                    -1.0f + 2.0f * line.endPoint.X / BaseGame.Width,
                    -(-1.0f + 2.0f * line.endPoint.Y / BaseGame.Height), 0),
                    line.color);
            }
            numOfPrimitives = numOfLines;

            // Vertex buffer was build
            buildVertexBuffer = false;
        }
        #endregion

        #region Add line
        /// <summary>
        /// Add line
        /// </summary>
        public void AddLine(Point startPoint, Point endPoint, Color color)
        {
            // Don't add new lines if limit is reached
            if (numOfLines >= MaxNumOfLines)
            {
                Log.Write("Too many lines requested in LineManager2D. " +
                    "Max lines = " + MaxNumOfLines);
                return;
            }

            // Build line
            Line line = new Line(startPoint, endPoint, color);

            // Check if this exact line exists at the current lines position.
            if (lines.Count > numOfLines)
            {
                if ((Line)lines[numOfLines] != line)
                {
                    // overwrite old line, otherwise just increase numOfLines
                    lines[numOfLines] = line;
                    // Remember to build vertex buffer in Render()
                    buildVertexBuffer = true;
                }
            }
            else
            {
                // Then just add new line
                lines.Add(line);
                // Remember to build vertex buffer in Render()
                buildVertexBuffer = true;
            }

            // nextUpValue line
            numOfLines++;
        }

        /// <summary>
        /// Add line with shadow
        /// </summary>
        /// <param name="startPoint">Start point</param>
        /// <param name="endPoint">End point</param>
        /// <param name="color">Color</param>
        public void AddLineWithShadow(Point startPoint, Point endPoint,
            Color color)
        {
            AddLine(new Point(startPoint.X, startPoint.Y + 1),
                new Point(endPoint.X, endPoint.Y + 1), Color.Black);
            AddLine(startPoint, endPoint, color);
        }
        #endregion

        #region Render
        /// <summary>
        /// Render all lines added this frame
        /// </summary>
        public virtual void Render()
        {
            // Need to build vertex buffer?
            if (buildVertexBuffer ||
                numOfPrimitives != numOfLines)
            {
                UpdateVertexBuffer();
            }

            // Render lines if we got any lines to render
            if (numOfPrimitives > 0)
            {
                BaseGame.SetAlphaBlendingEnabled(true);
                BaseGame.WorldMatrix = Matrix.Identity;
                ShaderEffect.lineRendering.Render(
                    "LineRendering2D",
                    delegate
                    {
                        BaseGame.Device.DrawUserPrimitives<VertexPositionColor>(
                            PrimitiveType.LineList, lineVertices, 0, numOfPrimitives);
                    });
            }

            // Ok, finally reset numOfLines for next frame
            numOfLines = 0;
        }
        #endregion
    }
}
