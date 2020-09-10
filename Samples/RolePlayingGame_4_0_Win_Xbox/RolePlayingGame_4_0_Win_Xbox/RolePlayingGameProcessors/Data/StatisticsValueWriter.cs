#region File Description
//-----------------------------------------------------------------------------
// StatisticsValueWriter.cs
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
    public class StatisticsValueWriter : RolePlayingGameWriter<StatisticsValue>
    {
        protected override void Write(ContentWriter output, StatisticsValue value)
        {
            output.Write(value.HealthPoints);
            output.Write(value.MagicPoints);
            output.Write(value.PhysicalOffense);
            output.Write(value.PhysicalDefense);
            output.Write(value.MagicalOffense);
            output.Write(value.MagicalDefense);
        }
    }
}
