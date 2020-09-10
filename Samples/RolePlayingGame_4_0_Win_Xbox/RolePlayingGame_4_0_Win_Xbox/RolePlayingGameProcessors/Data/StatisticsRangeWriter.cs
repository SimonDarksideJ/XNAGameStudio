#region File Description
//-----------------------------------------------------------------------------
// StatisticsRangeWriter.cs
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
    public class StatisticsRangeWriter : RolePlayingGameWriter<StatisticsRange>
    {
        protected override void Write(ContentWriter output, StatisticsRange value)
        {
            output.WriteObject(value.HealthPointsRange);
            output.WriteObject(value.MagicPointsRange);
            output.WriteObject(value.PhysicalOffenseRange);
            output.WriteObject(value.PhysicalDefenseRange);
            output.WriteObject(value.MagicalOffenseRange);
            output.WriteObject(value.MagicalDefenseRange);
        }
    }
}
