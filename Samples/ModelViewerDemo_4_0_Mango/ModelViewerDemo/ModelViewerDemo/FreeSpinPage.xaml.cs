#region File Description
//-----------------------------------------------------------------------------
// FreeSpinPage.xaml.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System.Diagnostics;

namespace ModelViewerDemo
{
    public partial class FreeSpinPage : BasePage
    {
        float yaw = 0f;
        float pitch = 45f;
        float distance = 2.5f;

        public FreeSpinPage()
        {
            InitializeComponent();

            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.Pinch;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Renderer.Current.State.AnimateCameraPosition(CalculateCameraPosition());
            Renderer.Current.State.AnimateCameraTarget(Vector3.Zero);

            base.OnNavigatedTo(e);
        }

        protected override void Update(GameTimerEventArgs e)
        {
            bool updateCamera = false;

            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample sample = TouchPanel.ReadGesture();

                if (sample.GestureType == GestureType.FreeDrag)
                {
                    yaw -= sample.Delta.X;
                    pitch = MathHelper.Clamp(pitch + sample.Delta.Y, 1f, 85f);
                    updateCamera = true;
                }
                else if (sample.GestureType == GestureType.Pinch)
                {
                    Vector2 oldA = sample.Position - sample.Delta;
                    Vector2 oldB = sample.Position2 - sample.Delta2;

                    float oldD = Vector2.Distance(oldA, oldB);
                    float newD = Vector2.Distance(sample.Position, sample.Position2);

                    float change = (oldD - newD) * .01f;

                    distance = MathHelper.Clamp(distance + change, 1f, 10f);

                    updateCamera = true;
                }
            }

            if (updateCamera)
            {
                Vector3 pos = CalculateCameraPosition();
                Renderer.Current.State.CameraX = pos.X;
                Renderer.Current.State.CameraY = pos.Y;
                Renderer.Current.State.CameraZ = pos.Z;
            }

            base.Update(e);
        }

        private Vector3 CalculateCameraPosition()
        {
            // start off with a position of (0, 0, 1)
            Vector3 position = new Vector3(0, 0, 1);

            // apply the yaw
            position = Vector3.Normalize(Vector3.Transform(position, Matrix.CreateRotationY(MathHelper.ToRadians(yaw))));

            // calculate the axis for the pitch
            Vector3 pitchAxis = Vector3.Cross(position, Vector3.Up);

            // apply the pitch
            position = Vector3.Normalize(Vector3.Transform(position, Matrix.CreateFromAxisAngle(pitchAxis, MathHelper.ToRadians(pitch))));

            // scale by the distance
            position *= distance;

            return position;
        }
    }
}