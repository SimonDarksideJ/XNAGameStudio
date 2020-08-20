#region File Description
//-----------------------------------------------------------------------------
// MonsterWriter.cs
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
    public class MonsterWriter : RolePlayingGameWriter<Monster>
    {
        FightingCharacterWriter fightingCharacterWriter = null;

        protected override void Initialize(ContentCompiler compiler)
        {
            fightingCharacterWriter = compiler.GetTypeWriter(typeof(FightingCharacter)) 
                as FightingCharacterWriter;

            base.Initialize(compiler);
        }

        protected override void Write(ContentWriter output, Monster value)
        {
            output.WriteRawObject<FightingCharacter>(value as FightingCharacter, 
                fightingCharacterWriter);
            output.Write(value.DefendPercentage);
            output.WriteObject(value.GearDrops);
        }
    }
}
