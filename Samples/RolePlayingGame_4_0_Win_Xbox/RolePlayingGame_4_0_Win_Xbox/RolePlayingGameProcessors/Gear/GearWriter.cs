#region File Description
//-----------------------------------------------------------------------------
// GearWriter.cs
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
    public class GearWriter : RolePlayingGameWriter<Gear>
    {
        protected override void Write(ContentWriter output, Gear value)
        {
            output.Write(value.Name);
            output.Write(value.Description);
            output.Write(value.GoldValue);
            output.Write(value.IsDroppable);
            output.Write(value.MinimumCharacterLevel);
            output.WriteObject(value.SupportedClasses);
            output.Write(value.IconTextureName);
        }
    }
}
