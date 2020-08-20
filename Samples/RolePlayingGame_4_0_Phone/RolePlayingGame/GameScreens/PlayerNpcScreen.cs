#region File Description
//-----------------------------------------------------------------------------
// PlayerNpcScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using RolePlayingGameData;
using Microsoft.Xna.Framework;
#endregion

namespace RolePlaying
{
    /// <summary>
    /// Displays the Player NPC screen, shown when encountering a player on the map.
    /// Typically, the user has an opportunity to invite the Player into the party.
    /// </summary>
    class PlayerNpcScreen : NpcScreen<Player>
    {
        /// <summary>
        /// If true, the NPC's introduction dialogue is shown.
        /// </summary>
        private bool isIntroduction = true;


        /// <summary>
        /// Constructs a new PlayerNpcScreen object.
        /// </summary>
        /// <param name="mapEntry"></param>
        public PlayerNpcScreen(MapEntry<Player> mapEntry)
            : base(mapEntry)
        {
            // assign and check the parameter
            Player playerNpc = character as Player;
            if (playerNpc == null)
            {
                throw new ArgumentException(
                    "PlayerNpcScreen requires a MapEntry with a Player");
            }

            this.DialogueText = playerNpc.IntroductionDialogue;
            this.BackText = "Reject";
            this.SelectText = "Accept";
            isIntroduction = true;
        }


        /// <summary>
        /// Handles user input.
        /// </summary>
        public override void HandleInput()
        {

            bool okClicked = false;
            bool backclicked = false;

            if (InputManager.IsButtonClicked(new Rectangle(
                (int)selectButtonPosition.X, (int)selectButtonPosition.Y,
                selectButtonTexture.Width, selectButtonTexture.Height)))
            {
                okClicked = true;
            }

            if (InputManager.IsButtonClicked(new Rectangle(
                (int)backButtonPosition.X, (int)backButtonPosition.Y,
                backButtonTexture.Width, backButtonTexture.Height)))
            {
                backclicked = true;
            }

            // view the player's statistics
            if (InputManager.IsActionTriggered(InputManager.Action.TakeView))
            {
                ScreenManager.AddScreen(new StatisticsScreen(character as Player));
                return;
            }

            if (isIntroduction)
            {
                // accept the invitation
                if (okClicked)
                {
                    isIntroduction = false;
                    Player player = character as Player;
                    Session.Party.JoinParty(player);
                    Session.RemovePlayerNpc(mapEntry);
                    this.DialogueText = player.JoinAcceptedDialogue;
                    this.BackText = "Back";
                    this.SelectText = "Continue";
                }
                // reject the invitation
                if (backclicked)
                {
                    isIntroduction = false;
                    Player player = character as Player;
                    this.DialogueText = player.JoinRejectedDialogue;
                    this.BackText = "Back";
                    this.SelectText = "Continue";
                }
            }
            else
            {
                // exit the screen
                if (okClicked ||backclicked)
                {
                    ExitScreen();
                    return;
                }
            }
        }
    }
}
