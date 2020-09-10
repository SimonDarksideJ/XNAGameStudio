#region File Description
//-----------------------------------------------------------------------------
// CustomModelAnimationSample.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using CustomModelAnimation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace CustomAvatarAnimationSample
{

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class CustomAvatarAnimationSampleGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;

        // Rigid model, animation players, clips
        Model rigidModel;
        Matrix rigidWorld;
        bool playingRigid;
        RootAnimationPlayer rigidRootPlayer;
        ModelAnimationClip rigidRootClip;
        RigidAnimationPlayer rigidPlayer;
        ModelAnimationClip rigidClip;

        // Skinned model, animation players, clips
        Model skinnedModel;
        Matrix skinnedWorld;
        bool playingSkinned;
        RootAnimationPlayer skinnedRootPlayer;
        ModelAnimationClip skinnedRootClip;
        SkinnedAnimationPlayer skinnedPlayer;
        ModelAnimationClip skinnedClip;

        // View and Projection matrices used for rendering 
        Matrix view;
        Matrix projection;

        SpriteBatch spriteBatch;
        SpriteFont font;

        // Store the current and last gamepad state
        GamePadState currentGamePadState;
        GamePadState lastGamePadState;

        // Store the current and last keyboard state
        KeyboardState currentKeyboardState;
        KeyboardState lastKeyboardState;

        #endregion


        #region Initialization

        /// <summary>
        /// Creates a new AvatarCustomAnimationSample object.
        /// </summary>
        public CustomAvatarAnimationSampleGame()
        {
            // initialize the graphics
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Load all graphical content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load the rigid model
            rigidModel = Content.Load<Model>("AnimatedCube");
            rigidWorld = Matrix.CreateScale(.05f, .05f, .05f);

            // Create animation players/clips for the rigid model
            ModelData modelData = rigidModel.Tag as ModelData;
            if (modelData != null)
            {
                if (modelData.RootAnimationClips != null && modelData.RootAnimationClips.ContainsKey("Take 001"))
                {
                    rigidRootClip = modelData.RootAnimationClips["Take 001"];

                    rigidRootPlayer = new RootAnimationPlayer();
                    rigidRootPlayer.Completed += new EventHandler(rigidPlayer_Completed);
                    rigidRootPlayer.StartClip(rigidRootClip, 1, TimeSpan.Zero);
                }
                if (modelData.ModelAnimationClips != null && modelData.ModelAnimationClips.ContainsKey("Take 001"))
                {
                    rigidClip = modelData.ModelAnimationClips["Take 001"];

                    rigidPlayer = new RigidAnimationPlayer(rigidModel.Bones.Count);
                    rigidPlayer.Completed += new EventHandler(rigidPlayer_Completed);
                    rigidPlayer.StartClip(rigidClip, 1, TimeSpan.Zero);
                }
            }

            // Load the skinned model
            skinnedModel = Content.Load<Model>("DudeWalk");
            skinnedWorld = Matrix.CreateScale(.025f, .025f, .025f) * Matrix.CreateRotationY((float)(-Math.PI / 2));

            // Create animation players for the skinned model
            modelData = skinnedModel.Tag as ModelData;
            if (modelData != null)
            {
                if (modelData.RootAnimationClips != null && modelData.RootAnimationClips.ContainsKey("Take 001"))
                {
                    skinnedRootClip = modelData.RootAnimationClips["Take 001"];

                    skinnedRootPlayer = new RootAnimationPlayer();
                    skinnedRootPlayer.Completed += new EventHandler(skinnedPlayer_Completed);
                }
                if (modelData.ModelAnimationClips != null && modelData.ModelAnimationClips.ContainsKey("Take 001"))
                {
                    skinnedClip = modelData.ModelAnimationClips["Take 001"];

                    skinnedPlayer = new SkinnedAnimationPlayer(modelData.BindPose, modelData.InverseBindPose, modelData.SkeletonHierarchy);
                    skinnedPlayer.Completed += new EventHandler(skinnedPlayer_Completed);
                }
            }

            // Create the projection/view matrix we'll use for rendering
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), GraphicsDevice.Viewport.AspectRatio, .01f, 200.0f);
            view = Matrix.CreateLookAt(new Vector3(0, 1, 4), new Vector3(0, 1, 0), Vector3.Up);

            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("font");
        }

        /// <summary>
        /// Callback function when the a skinned animation player is completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void skinnedPlayer_Completed(object sender, EventArgs e)
        {
            playingSkinned = false;
        }

        /// <summary>
        /// Callback function when a rigid animation player is completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void rigidPlayer_Completed(object sender, EventArgs e)
        {
            playingRigid = false;
        }

        #endregion


        #region Updating

        protected override void Update(GameTime gameTime)
        {
            // Get the current gamepad state and store the old
            lastGamePadState = currentGamePadState;
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            lastKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            // Allows the game to exit
            if (currentGamePadState.Buttons.Back == ButtonState.Pressed ||
                currentKeyboardState.IsKeyDown(Keys.Escape))
                Exit();

            // When the A button is pressed and we aren't playing the rigid animations, play them
            if ((IsNewButtonPress(Buttons.A) || IsNewKeyPress(Keys.A)) && playingRigid == false)
            {
                if (rigidPlayer != null && rigidClip != null)
                {
                    rigidPlayer.StartClip(rigidClip, 1, TimeSpan.Zero);
                    playingRigid = true;
                }

                if (rigidRootPlayer != null && rigidRootClip != null)
                {
                    rigidRootPlayer.StartClip(rigidRootClip, 1, TimeSpan.Zero);
                    playingRigid = true;
                }
            }

            // When the B button is pressed and we aren't playing the skinned animations, play them
            if ((IsNewButtonPress(Buttons.B) || IsNewKeyPress(Keys.B)) && playingSkinned == false)
            {
                if (skinnedPlayer != null && skinnedClip != null)
                {
                    skinnedPlayer.StartClip(skinnedClip, 1, TimeSpan.Zero);
                    playingSkinned = true;
                }

                if (skinnedRootPlayer != null && skinnedRootClip != null)
                {
                    skinnedRootPlayer.StartClip(skinnedRootClip, 1, TimeSpan.Zero);
                    playingSkinned = true;
                }
            }

            // If we are playing rigid animations, update the players
            if (playingRigid)
            {
                if (rigidRootPlayer != null)
                    rigidRootPlayer.Update(gameTime);

                if (rigidPlayer != null)
                    rigidPlayer.Update(gameTime);
            }

            // If we are playing skinned animations, update the players
            if (playingSkinned)
            {
                if (skinnedRootPlayer != null)
                    skinnedRootPlayer.Update(gameTime);

                if (skinnedPlayer != null)
                    skinnedPlayer.Update(gameTime);
            }

            base.Update(gameTime);
        }

        // Helper method to tell if a button was just pressed
        bool IsNewButtonPress(Buttons button)
        {
            return currentGamePadState.IsButtonDown(button) && lastGamePadState.IsButtonUp(button);
        }

        // Helper method to tell if a key was just pressed
        bool IsNewKeyPress(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key) && lastKeyboardState.IsKeyUp(key);
        }


        #endregion


        #region Drawing

        protected override void Draw(GameTime gameTime)
        {
            // Reset the rendering states changed by spriteBatch
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (playingRigid)
                DrawRigidModel(rigidModel, rigidPlayer, rigidRootPlayer);

            if (playingSkinned)
                DrawSkinnedModel(skinnedModel, skinnedPlayer, skinnedRootPlayer);


            base.Draw(gameTime);

            DrawHUD();
        }

        private void DrawHUD()
        {
            string controls = "Controls: \n";
            controls += "Press A to Play the Rigid Animation\n";
            controls += "Press B to Play the Skinned Animation\n";

            string status = "Animation Status:\n";
            status += "Rigid: ";
            if (playingRigid == false)
                status += "Stopped\n";
            else
                status += "Playing\n";
            status += "Skinned: ";
            if (playingSkinned == false)
                status += "Stopped\n";
            else
                status += "Playing\n";

            spriteBatch.Begin();
            spriteBatch.DrawString(font, controls, new Vector2(100, 80), Color.White);
            spriteBatch.DrawString(font, status, new Vector2(100, 200), Color.White);
            spriteBatch.End();
        }

        private void DrawSkinnedModel(Model model, SkinnedAnimationPlayer skinnedAnimationPlayer, RootAnimationPlayer rootAnimationPlayer)
        {
            Matrix[] boneTransforms = null;
            if (skinnedAnimationPlayer != null)
                boneTransforms = skinnedAnimationPlayer.GetSkinTransforms();

            Matrix rootTransform = Matrix.Identity;
            if (rootAnimationPlayer != null)
                rootTransform = rootAnimationPlayer.GetCurrentTransform();

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.Projection = projection;
                    effect.View = view;
                    if (boneTransforms != null)
                        effect.SetBoneTransforms(boneTransforms);
                    effect.World = rootTransform * skinnedWorld;
                    effect.SpecularColor = Vector3.Zero;
                }

                mesh.Draw();
            }
        }

        private void DrawRigidModel(Model model, RigidAnimationPlayer rigidAnimationPlayer, RootAnimationPlayer rootAnimationPlayer)
        {
            Matrix[] boneTransforms = null;
            if (rigidAnimationPlayer != null)
                boneTransforms = rigidAnimationPlayer.GetBoneTransforms();

            Matrix rootTransform = Matrix.Identity;
            if (rootAnimationPlayer != null)
                rootTransform = rootAnimationPlayer.GetCurrentTransform();

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.Projection = projection;
                    effect.View = view;
                    if (boneTransforms != null)
                        effect.World = boneTransforms[mesh.ParentBone.Index] * rootTransform * rigidWorld;
                    else
                        effect.World = rootTransform * rigidWorld;
                }

                mesh.Draw();
            }
        }

        #endregion


        #region Entry Point

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (CustomAvatarAnimationSampleGame game = new CustomAvatarAnimationSampleGame())
            {
                game.Run();
            }
        }

        #endregion
    }
}
