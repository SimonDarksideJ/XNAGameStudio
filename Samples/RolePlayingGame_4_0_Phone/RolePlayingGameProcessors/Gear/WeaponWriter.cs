#region File Description
//-----------------------------------------------------------------------------
// WeaponWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using RolePlayingGameData;
#endregion

namespace RolePlayingGameProcessors
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentTypeWriter]
    public class WeaponWriter : RolePlayingGameWriter<Weapon>
    {
        EquipmentWriter equipmentWriter = null;

        protected override void Initialize(ContentCompiler compiler)
        {
            equipmentWriter = compiler.GetTypeWriter(typeof(Equipment))
                as EquipmentWriter;

            base.Initialize(compiler);
        }

        protected override void Write(ContentWriter output, Weapon value)
        {
            // write out equipment values
            output.WriteRawObject<Equipment>(value as Equipment, equipmentWriter);

            output.WriteObject(value.TargetDamageRange);
            output.Write(value.SwingCueName);
            output.Write(value.HitCueName);
            output.Write(value.BlockCueName);
            output.WriteObject(value.Overlay);
        }
    }
}
