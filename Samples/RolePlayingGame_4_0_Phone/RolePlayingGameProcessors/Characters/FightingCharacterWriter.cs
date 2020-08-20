#region File Description
//-----------------------------------------------------------------------------
// FightingCharacterWriter.cs
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
    public class FightingCharacterWriter : RolePlayingGameWriter<FightingCharacter>
    {
        CharacterWriter characterWriter = null;

        protected override void Initialize(ContentCompiler compiler)
        {
            characterWriter = compiler.GetTypeWriter(typeof(Character)) 
                as CharacterWriter;

            base.Initialize(compiler);
        }

        protected override void Write(ContentWriter output, FightingCharacter value)
        {
            output.WriteRawObject<Character>(value as Character, characterWriter);
            output.Write(value.CharacterClassContentName);
            output.Write(value.CharacterLevel);
            output.WriteObject(value.InitialEquipmentContentNames);
            output.WriteObject(value.Inventory);
            output.Write(value.CombatAnimationInterval);
            output.WriteObject(value.CombatSprite);
        }
    }
}
