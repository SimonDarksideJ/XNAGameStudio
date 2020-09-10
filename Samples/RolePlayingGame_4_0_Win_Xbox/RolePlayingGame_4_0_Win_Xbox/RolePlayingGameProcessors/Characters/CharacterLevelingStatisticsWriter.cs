#region File Description
//-----------------------------------------------------------------------------
// CharacterLevelingStatisticsWriter.cs
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
    public class CharacterLevelingStatisticsWriter :
        RolePlayingGameWriter<CharacterLevelingStatistics>
    {
        protected override void Write(ContentWriter output, 
            CharacterLevelingStatistics value)
        {
            output.Write(value.HealthPointsIncrease);
            output.Write(value.MagicPointsIncrease);
            output.Write(value.PhysicalOffenseIncrease);
            output.Write(value.PhysicalDefenseIncrease);
            output.Write(value.MagicalOffenseIncrease);
            output.Write(value.MagicalDefenseIncrease);

            output.Write(value.LevelsPerHealthPointsIncrease);
            output.Write(value.LevelsPerMagicPointsIncrease);
            output.Write(value.LevelsPerPhysicalOffenseIncrease);
            output.Write(value.LevelsPerPhysicalDefenseIncrease);
            output.Write(value.LevelsPerMagicalOffenseIncrease);
            output.Write(value.LevelsPerMagicalDefenseIncrease);
        }
    }
}
