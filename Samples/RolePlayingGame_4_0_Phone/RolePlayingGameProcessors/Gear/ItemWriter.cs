#region File Description
//-----------------------------------------------------------------------------
// ItemWriter.cs
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
    public class ItemWriter : RolePlayingGameWriter<Item>
    {
        GearWriter gearWriter = null;

        protected override void Initialize(ContentCompiler compiler)
        {
            gearWriter = compiler.GetTypeWriter(typeof(Gear)) as GearWriter;

            base.Initialize(compiler);
        }

        protected override void Write(ContentWriter output, Item value)
        {
            // write out gear values
            output.WriteRawObject<Gear>(value as Gear, gearWriter);

            // write out item values
            output.Write((Int32)value.Usage);
            output.Write(value.IsOffensive);
            output.Write(value.TargetDuration);
            output.WriteObject(value.TargetEffectRange);
            output.Write(value.AdjacentTargets);
            output.Write(value.UsingCueName);
            output.Write(value.TravelingCueName);
            output.Write(value.ImpactCueName);
            output.Write(value.BlockCueName);
            output.WriteObject(value.CreationSprite);
            output.WriteObject(value.SpellSprite);
            output.WriteObject(value.Overlay);
        }
    }
}
