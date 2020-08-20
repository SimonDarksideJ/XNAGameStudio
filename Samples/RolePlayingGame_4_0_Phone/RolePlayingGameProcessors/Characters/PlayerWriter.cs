#region File Description
//-----------------------------------------------------------------------------
// PlayerWriter.cs
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
    public class PlayerWriter : RolePlayingGameWriter<Player>
    {
        FightingCharacterWriter fightingCharacterWriter = null;

        protected override void Initialize(ContentCompiler compiler)
        {
            fightingCharacterWriter = compiler.GetTypeWriter(typeof(FightingCharacter))
                as FightingCharacterWriter;

            base.Initialize(compiler);
        }

        protected override void Write(ContentWriter output, Player value)
        {
            output.WriteRawObject<FightingCharacter>(value as FightingCharacter, 
                fightingCharacterWriter);
            output.Write(value.Gold);
            output.Write(value.IntroductionDialogue);
            output.Write(value.JoinAcceptedDialogue);
            output.Write(value.JoinRejectedDialogue);
            output.Write(value.ActivePortraitTextureName);
            output.Write(value.InactivePortraitTextureName);
            output.Write(value.UnselectablePortraitTextureName);
        }
    }
}
