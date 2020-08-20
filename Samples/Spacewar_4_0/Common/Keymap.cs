#region File Description
//-----------------------------------------------------------------------------
// Keymap.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Diagnostics;
#endregion

namespace Spacewar
{
    class Keymap
    {

        private Dictionary<GamePadKeys, List<Keys>> bindings;
        // currently a hack so I can bind the same keyboard key to multiple gamepad keys



        public Keymap()
        {
            bindings = new Dictionary<GamePadKeys, List<Keys>>();
        }


        public void Add(GamePadKeys gk, Keys k)
        {

            List<Keys> keyboardkey = new List<Keys>();
            keyboardkey.Add(k);


            if (bindings.ContainsKey(gk))
            {

                bindings.Remove(gk);
                bindings.Add(gk, keyboardkey);

            }
            else
            {
                bindings.Add(gk, keyboardkey);
            }

        }


        public Keys Get(GamePadKeys gk)
        {
            return bindings[gk][0];
        }

    }
}
