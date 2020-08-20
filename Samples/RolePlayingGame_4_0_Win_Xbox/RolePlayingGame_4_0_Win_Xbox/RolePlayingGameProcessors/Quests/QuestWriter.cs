#region File Description
//-----------------------------------------------------------------------------
// QuestWriter.cs
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
    public class QuestWriter : RolePlayingGameWriter<Quest>
    {
        protected override void Write(ContentWriter output, Quest value)
        {
            output.Write(value.Name);
            output.Write(value.Description);
            output.Write(value.ObjectiveMessage);
            output.Write(value.CompletionMessage);
            output.WriteObject(value.GearRequirements);
            output.WriteObject(value.MonsterRequirements);
            output.WriteObject(value.FixedCombatEntries);
            output.WriteObject(value.ChestEntries);
            output.Write(value.DestinationMapContentName);
            output.Write(value.DestinationNpcContentName);
            output.Write(value.DestinationObjectiveMessage);
            output.Write(value.ExperienceReward);
            output.Write(value.GoldReward);
            output.WriteObject(value.GearRewardContentNames);
        }
    }
}
