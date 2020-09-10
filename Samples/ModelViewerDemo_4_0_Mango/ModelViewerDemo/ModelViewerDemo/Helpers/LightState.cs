#region File Description
//-----------------------------------------------------------------------------
// LightState.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ModelViewerDemo
{
    /// <summary>
    /// Implement our light state on top of a DependencyObject which enables us to use databinding for
    /// UI interaction or animation through a Storyboard.
    /// </summary>
    public class LightState : DependencyObject
    {
        private Vector3 defaultDiffuseColor;
        private Vector3 defaultDirection;
        private bool defaultEnabled;
        private Vector3 defaultSpecular;

        public static readonly DependencyProperty DiffuseRProperty =
            DependencyProperty.Register("DiffuseR", typeof(double), typeof(LightState), new PropertyMetadata(0.0));
        public static readonly DependencyProperty DiffuseGProperty =
            DependencyProperty.Register("DiffuseG", typeof(double), typeof(LightState), new PropertyMetadata(0.0));
        public static readonly DependencyProperty DiffuseBProperty =
            DependencyProperty.Register("DiffuseB", typeof(double), typeof(LightState), new PropertyMetadata(0.0));

        public static readonly DependencyProperty DirectionXProperty =
            DependencyProperty.Register("DirectionX", typeof(double), typeof(LightState), new PropertyMetadata(0.0));
        public static readonly DependencyProperty DirectionYProperty =
            DependencyProperty.Register("DirectionY", typeof(double), typeof(LightState), new PropertyMetadata(0.0));
        public static readonly DependencyProperty DirectionZProperty =
            DependencyProperty.Register("DirectionZ", typeof(double), typeof(LightState), new PropertyMetadata(0.0));

        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.Register("Enabled", typeof(bool), typeof(LightState), new PropertyMetadata(true));

        public static readonly DependencyProperty SpecularRProperty =
            DependencyProperty.Register("SpecularR", typeof(double), typeof(LightState), new PropertyMetadata(0.0));
        public static readonly DependencyProperty SpecularGProperty =
            DependencyProperty.Register("SpecularG", typeof(double), typeof(LightState), new PropertyMetadata(0.0));
        public static readonly DependencyProperty SpecularBProperty =
            DependencyProperty.Register("SpecularB", typeof(double), typeof(LightState), new PropertyMetadata(0.0));

        public double DiffuseR
        {
            get { return (double)GetValue(DiffuseRProperty); }
            set { SetValue(DiffuseRProperty, value); }
        }

        public double DiffuseG
        {
            get { return (double)GetValue(DiffuseGProperty); }
            set { SetValue(DiffuseGProperty, value); }
        }

        public double DiffuseB
        {
            get { return (double)GetValue(DiffuseBProperty); }
            set { SetValue(DiffuseBProperty, value); }
        }

        public double DirectionX
        {
            get { return (double)GetValue(DirectionXProperty); }
            set { SetValue(DirectionXProperty, value); }
        }

        public double DirectionY
        {
            get { return (double)GetValue(DirectionYProperty); }
            set { SetValue(DirectionYProperty, value); }
        }

        public double DirectionZ
        {
            get { return (double)GetValue(DirectionZProperty); }
            set { SetValue(DirectionZProperty, value); }
        }

        public Vector3 Direction
        {
            get
            {
                return new Vector3(
                    (float)DirectionX,
                    (float)DirectionY,
                    (float)DirectionZ);
            }
        }

        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }

        public double SpecularR
        {
            get { return (double)GetValue(SpecularRProperty); }
            set { SetValue(SpecularRProperty, value); }
        }

        public double SpecularG
        {
            get { return (double)GetValue(SpecularGProperty); }
            set { SetValue(SpecularGProperty, value); }
        }

        public double SpecularB
        {
            get { return (double)GetValue(SpecularBProperty); }
            set { SetValue(SpecularBProperty, value); }
        }

        public void ResetToDefaults()
        {
            DiffuseR = defaultDiffuseColor.X;
            DiffuseG = defaultDiffuseColor.Y;
            DiffuseB = defaultDiffuseColor.Z;

            DirectionX = defaultDirection.X;
            DirectionY = defaultDirection.Y;
            DirectionZ = defaultDirection.Z;

            Enabled = defaultEnabled;

            SpecularR = defaultSpecular.X;
            SpecularG = defaultSpecular.Y;
            SpecularB = defaultSpecular.Z;
        }

        public void ReadDataFrom(DirectionalLight light)
        {
            defaultDiffuseColor = light.DiffuseColor;
            defaultDirection = light.Direction;
            defaultSpecular = light.SpecularColor;
            defaultEnabled = light.Enabled;

            ResetToDefaults();
        }

        public void Apply(DirectionalLight light)
        {
            light.DiffuseColor = new Vector3(
                (float)DiffuseR,
                (float)DiffuseG,
                (float)DiffuseB);

            light.Direction = new Vector3(
                (float)DirectionX,
                (float)DirectionY,
                (float)DirectionZ);

            light.Enabled = Enabled;

            light.SpecularColor = new Vector3(
                (float)SpecularR,
                (float)SpecularG,
                (float)SpecularB);
        }
    }
}
