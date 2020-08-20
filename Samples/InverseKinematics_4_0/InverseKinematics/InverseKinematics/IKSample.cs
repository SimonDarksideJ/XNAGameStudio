#region File Description
//-----------------------------------------------------------------------------
// IKSample.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace InverseKinematicsSample
{
    /// <summary>
    /// This sample demonstrates how to update an IK chain using the Cyclic Coordinate
    /// Descent algorithm (CCD). It also demonstrates how to hook up that IK chain to
    /// an avatar model.
    /// </summary>
    public class IKSample : Microsoft.Xna.Framework.Game
    {
        #region Fields and Constants

        //The number of links in the cylinder chain
        const int CylinderCount = 20;

        // Rendering stuff
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Model cylinderModel;
        SpriteFont font;

        // Keeps track of the input for this sample
        KeyboardState currentKeyboardState;
        GamePadState currentGamePadState;
        GamePadState prevGamePadState;
        KeyboardState prevKeyboardState;

        // Camera related controls
        float cameraRotX;
        float cameraRotY;
        float cameraRadius = 5;
        Matrix view;
        Matrix projection;



        /// <summary>
        /// The player controlled object that the IK chain attempts to reach.
        /// </summary>
        Cat cat;

        /// <summary>
        /// When set to true, the simulation will run continuously.
        /// </summary>
        bool runSimulation = true;

        /// <summary>
        /// Allows the player to step through each bone update of the IK chain. 
        /// By default, the entire IK chain is updated once per frame.
        /// </summary>
        bool singleStep;



        /// <summary>
        /// The avatar renderer object used for drawing the avatar and 
        /// accessing the avatar's bind pose.
        /// </summary>
        AvatarRenderer avatarRenderer;

        /// <summary>
        /// The list of bones to be updated as part of the IK chain. The order 
        /// and number of bones in the IK chain will affect the way it animates to reach 
        /// the cat. The default IK chain for the avatar in this sample is: 
        /// FingerMiddle3Left, WristLeft, ElbowLeft, and ShoulderLeft.
        /// </summary>
        List<int> avatarBoneChain = new List<int>();
        
        /// <summary>
        /// The list of bone transformation offsets from the avatar's bind pose. This 
        /// list is akin to the AvatarAnimation.BoneTransforms. This list of transform
        /// matricies will be used to save the rotation information that moves the end
        /// effector toward the cat.
        /// </summary>
        List<Matrix> avatarBoneTransforms;

        /// <summary>
        /// Stores the entire list of world transforms for the avatar
        /// </summary>
        List<Matrix> avatarWorldTransforms;

        /// <summary>
        /// The list of local transforms for the avatar. The bones relevant to the
        /// IK chain are updated and stored in this list.
        /// </summary>
        List<Matrix> avatarLocalTransforms;

        /// <summary>
        /// The index into the avatarBoneChain for the currently updating bone.
        /// </summary>
        int avatarChainIndex = 1;

        /// <summary>
        /// Used for initializing the avatar transforms once.
        /// </summary>
        bool isAvatarInitialized = false;



        /// <summary>
        /// The list of bones to be updated as part of the IK chain. The order and 
        /// number of bones in the IK chain will affect the way it animates to reach 
        /// the cat.
        /// </summary>
        List<int> cylinderChain;

        /// <summary>
        /// The list of default position and orientation of the cylinder IK chain bones. 
        /// These positions and orientations are in local space, and are relative to the
        /// parent bone.
        /// </summary>
        List<Matrix> cylinderChainBindPose = new List<Matrix>();

        /// <summary>
        /// The list of bone transformation offsets from the cylinder chain's bind pose. 
        /// This list is akin to the AvatarAnimation.BoneTransforms. This list of
        /// transform matricies will be used to save the rotation information that 
        /// moves the end effector toward the cat.
        /// </summary>
        List<Matrix> cylinderChainTransforms;

        /// <summary>
        /// The collection of the parent indices for each bone in the related 
        /// cylinderChainBindPose collection. This is identical to the implementation 
        /// of the AvatarRenderer.ParentBones.
        /// </summary>
        List<int> cylinderChainParentBones;

        /// <summary>
        /// Stores the entire list of world transforms for the cylinder chain
        /// </summary>
        List<Matrix> cylinderWorldTransforms;

        /// <summary>
        /// The list of local transforms for the cylinder chain. The bones relevant 
        /// to the IK chain are updated and stored in this list.
        /// </summary>
        List<Matrix> cylinderLocalTransforms;

        /// <summary>
        /// The world transform of the cylinder.
        /// </summary>
        Matrix cylinderRootWorldTransform = Matrix.Identity;

        /// <summary>
        /// The index into the cylinderChain for the currently updating bone.
        /// </summary>
        int cylinderChainIndex = 1;


        #endregion

        #region Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        public IKSample()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 853;
            graphics.PreferredBackBufferHeight = 480;
            graphics.PreferMultiSampling = true;

            Components.Add(new GamerServicesComponent(this));

            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Creates the IK chains for the avatar and they cylinder chain
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("font");

            LoadCylinderModel();

            // Create the cat
            cat = new Cat(GraphicsDevice)
            {
                Scale = .3f,
                Position = new Vector3(-1, .25f, -2),
                Texture = Content.Load<Texture2D>("cat"),
            };

            LoadAvatar();
            InitializeCylinderChain();
        }

        /// <summary>
        /// Load the cylinder model and configure its BasicEffect
        /// </summary>
        private void LoadCylinderModel()
        {
            //Load cylinder model
            cylinderModel = Content.Load<Model>("cylinder");

            //Set the basic effect lighting for the cylinder model
            foreach (ModelMesh mesh in cylinderModel.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    BasicEffect basicEffect = effect as BasicEffect;
                    if (basicEffect != null)
                    {
                        basicEffect.EnableDefaultLighting();
                        basicEffect.PreferPerPixelLighting = true;
                    }
                }
            }
        }

        /// <summary>
        /// Load the avatar and initialize the avatar IK chain.
        /// </summary>
        private void LoadAvatar()
        {
            // Create a new avatar renderer.
            avatarRenderer = new AvatarRenderer(AvatarDescription.CreateRandom(), true);

            // Create the avatar IK chain.
            avatarBoneChain.Clear();
            avatarBoneChain.Add((int)AvatarBone.FingerMiddle3Left);
            avatarBoneChain.Add((int)AvatarBone.WristLeft);
            avatarBoneChain.Add((int)AvatarBone.ElbowLeft);
            avatarBoneChain.Add((int)AvatarBone.ShoulderLeft);

            // Initialize the avatar transform lists to the identity.
            int boneCount = AvatarRenderer.BoneCount;
            avatarBoneTransforms = 
                Enumerable.Repeat(Matrix.Identity, boneCount).ToList();
            avatarWorldTransforms = avatarBoneTransforms.ToList();
            avatarLocalTransforms = avatarBoneTransforms.ToList();

            // Rotate the right arm down so it's idle at the avatar's hip.
            avatarBoneTransforms[(int)AvatarBone.ShoulderRight] = 
                Matrix.CreateRotationZ( MathHelper.ToRadians(80));

            // Position the avatar.
            avatarRenderer.World = Matrix.CreateTranslation(1, 0, 0);            
        }

        /// <summary>
        /// Create and initialize the cylinder IK chain
        /// </summary>
        private void InitializeCylinderChain()
        {
            // Initialize chain bind pose.
            Matrix T = Matrix.CreateTranslation(0, .1f, 0);
            cylinderChainBindPose = Enumerable.Repeat(T, CylinderCount).ToList();

            // Initialize the chain transform lists to the identity
            cylinderChainTransforms = 
                Enumerable.Repeat(Matrix.Identity, CylinderCount).ToList();
            cylinderWorldTransforms = cylinderChainTransforms.ToList();
            cylinderLocalTransforms = cylinderChainTransforms.ToList();

            // Initialize the parent index list. For the cylinder chain, the parent bone 
            // is the one just before it in the list. This gives us a list of parent 
            // bones like this : {-1, 0, 1, 2, 3, ..., chainBindPose.Count - 2}. Each 
            // number is the parent bone index into cylinderChainBindPose and 
            // cylinderChainTransforms. This is identical to the implementation of 
            // the AvatarRenderer class.
            cylinderChainParentBones = Enumerable.Range(-1, CylinderCount).ToList();

            // Initialize the IK chain. This will give an IK chain that looks like this:
            // {CylinderCount - 1, CylinderCount - 2, ..., 1, 0}. The bone index at 
            // cylinderChain[0] is used as the end effector of the IK chain. Each 
            // number is the bone index into cylinderChainBindPose 
            // and cylinderChainTransforms.
            cylinderChain = Enumerable.Range(0, CylinderCount).Reverse().ToList();

            //Initialize the World and Local transform lists
            UpdateTransforms(cylinderWorldTransforms, cylinderLocalTransforms,
                cylinderRootWorldTransform, cylinderChainBindPose, 
                cylinderChainTransforms, cylinderChainParentBones);

        }

        #endregion

        #region Update

        /// <summary>
        /// Allows the game logic to run
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            HandleInput(gameTime);

            UpdateCamera();

            // Only run the IK simulation if the user has unpaused the simulation or 
            // chosen to step through it.
            if (runSimulation || IsTriggered(Buttons.B) || IsTriggered(Keys.Space))
            {
                UpdateAvatarIK();
                UpdateCylinderChainIK();
            }           

            base.Update(gameTime);        
        }

        /// <summary>
        /// Update the view and projection matrices.
        /// </summary>
        private void UpdateCamera()
        {
            view = Matrix.CreateRotationY(MathHelper.ToRadians(cameraRotY)) *
                   Matrix.CreateRotationX(MathHelper.ToRadians(cameraRotX)) *
                   Matrix.CreateLookAt(new Vector3(0, 0, -cameraRadius),
                                      new Vector3(0, 0, 0), Vector3.Up);

            float aspectRatio = GraphicsDevice.Viewport.AspectRatio;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45),
                                                                    aspectRatio,
                                                                    1,
                                                                    1000);
        }

        #endregion

        #region Inverse Kinematics

        /// <summary>
        /// Loop over the Avatar IK chain and update each bone. The goal is to bring the 
        /// end effector closer to the cat
        /// </summary>
        private void UpdateAvatarIK()
        {
            if ( avatarRenderer.State != AvatarRendererState.Ready)
            {
                isAvatarInitialized = false;
            }
            else
            {
                // We need to initialize the avatars transforms once it's loaded
                if (isAvatarInitialized == false)
                {
                    UpdateTransforms(avatarWorldTransforms, avatarLocalTransforms,
                        avatarRenderer.World, avatarRenderer.BindPose,
                        avatarBoneTransforms, avatarRenderer.ParentBones);
                    isAvatarInitialized = true;
                }

                //Make the avatar look at the cat
                AvatarLookAt(cat.Position);

                // Update avatar chain
                while (avatarChainIndex < avatarBoneChain.Count)
                {
                    // Update the current IK bone.
                    UpdateBone(avatarBoneTransforms, avatarBoneChain[avatarChainIndex],
                        avatarBoneChain[0], cat.Position,
                        avatarWorldTransforms, avatarLocalTransforms);

                    // Update the local and world transforms for the IK chain now that 
                    // bones have moved.
                    UpdateTransforms(avatarWorldTransforms, avatarLocalTransforms,
                        avatarRenderer.World, avatarRenderer.BindPose,
                        avatarBoneTransforms, avatarRenderer.ParentBones);

                    // Move to the next bone.
                    ++avatarChainIndex;

                    // If the user wants to view each bone update one at a time, we
                    // exit the loop here.
                    if (singleStep)
                        break;
                }

                // Reset the IK chain index to one after the end effector
                if (avatarChainIndex >= avatarBoneChain.Count)
                    avatarChainIndex = 1;
            }
        }
        
        /// <summary>
        /// Makes the avatar look at a position in world space.
        /// </summary>
        /// <param name="position">The position in world space to look at</param>
        private void AvatarLookAt(Vector3 position)
        {
            int headIndex = (int)AvatarBone.Head;
            Vector3 target = position - avatarWorldTransforms[headIndex].Translation;
            target.X = -target.X; //Flip the X axis.

            Matrix lookAt = Matrix.CreateLookAt( Vector3.Zero, target, Vector3.Up);

            avatarBoneTransforms[headIndex] = lookAt;
        }

        /// <summary>
        /// Loop over the cylinder IK chain and update each bone to bring the 
        /// end effector closer to the cat
        /// </summary>
        private void UpdateCylinderChainIK()
        {
            // Update cylinder chain
            while (cylinderChainIndex < cylinderChain.Count)
            {
                // Update the current IK bone
                UpdateBone(cylinderChainTransforms, cylinderChain[cylinderChainIndex], 
                    cylinderChain[0], cat.Position,
                    cylinderWorldTransforms, cylinderLocalTransforms);

                // Update the local and world transforms for the IK chain now that 
                // bones have moved.
                UpdateTransforms(cylinderWorldTransforms, cylinderLocalTransforms, 
                    cylinderRootWorldTransform, cylinderChainBindPose, 
                    cylinderChainTransforms, cylinderChainParentBones);

                // Move to the next bone
                ++cylinderChainIndex;

                // If the user wants to view each bone update one at a time, we
                // exit the loop her.
                if (singleStep)
                    break;
            }

            // Reset the IK chain index to one after the end effector
            if (cylinderChainIndex >= cylinderChain.Count)
                cylinderChainIndex = 1;
        }

        /// <summary>
        /// This is the primary function for updating inverse kinematics. Here, we 
        /// implement the Cyclic Coordinate Decsent algorithm. In a nutshell this is 
        /// what we are trying to do: Given a goal position, end effector (often 
        /// the end bone in a chain), and a current bone, we want to rotate the current 
        /// bone such that it will bring the end effector closer to the goal position. 
        /// We do this iteratively for every bone in the chain.
        /// 
        /// Here's a very basic idea of how the algorithm works:
        /// 1) Compute vector directions for the bone to 
        ///    the goal and the bone to end effector.
        /// 2) Compute a matrix that rotates the end effector vector onto 
        ///    the goal vector.
        /// 3) Rotate the current bone by this rotation matrix.
        /// 4) Repeat steps 1-3 for each bone in the chain.
        /// </summary>
        /// <param name="curBone">The index into the bone chain for the current 
        /// bone to update.</param>
        /// <param name="endEffector">The index of the end effector in bone chain. The 
        /// end effector is the end bone that we want to move toward the cat.</param>
        /// <param name="goal">The world position of the object the IK chain is trying 
        /// to move to.</param>
        /// <param name="rootWorldTransform">The world transform for the root of 
        /// the bone chain</param>
        /// <param name="bindPose">The bind pose of the IK objects or the default 
        /// position and orientation of the bones in the IK chain</param>
        /// <param name="transforms">The list of rotational offsets from
        /// the bind pose</param>
        /// <param name="parentBones">The list of parent bones for each 
        /// bone index.</param>
        static private void UpdateBone(IList<Matrix> transforms, int curBone,
            int endEffector, Vector3 goal, IList<Matrix> worldTransforms, 
            IList<Matrix> localTransforms)
        {

            // We first compute the vector directions for the current bone to the goal 
            // and the current bone to end effector. We will do all this in the current 
            // bone's local coordinates which makes it easier to generate our final 
            // rotation matrix for the bone.

            // Get the world transform of the current bone
            Matrix curBoneWorld = worldTransforms[curBone];

            // Transform the goal into coordinate system of current bone. To do this, we
            // transform the goal position by the inverse world transform of the 
            // current bone.
            Vector3 goalInBoneSpace = Vector3.Transform( goal, 
                                                         Matrix.Invert(curBoneWorld));

            // Transform the end effector into coordinate system of the current bone. To 
            // do this, we first compute the current world transform of the end effector 
            // then we multiply it by the Inverse of the current bone's world transform. 
            // We then store the position.
            Matrix endEffectorWorld = worldTransforms[endEffector];
            Vector3 endEffectorInBoneSpace = Matrix.Multiply(endEffectorWorld,
                                               Matrix.Invert(curBoneWorld)).Translation;

            // After we normalize, we will have unit vectors that represent the 
            // direction to the end effector and the goal in the local coordinate space 
            // of the current bone.
            endEffectorInBoneSpace.Normalize();
            goalInBoneSpace.Normalize();

            // Next we build the rotation matrix that rotates the end effector onto the
            // goal and apply that rotation to the current bone.

            // Compute axis of rotation: the cross product of the two vectors.
            Vector3 axis = Vector3.Cross(endEffectorInBoneSpace, goalInBoneSpace);

            // Use TransformNormal to orient the axis by the local coordinate
            // transform of the current bone
            axis = Vector3.TransformNormal(axis, localTransforms[curBone]);
            axis.Normalize();

            // Compute the angle we will be rotating by which is just the angle 
            // between the vectors
            float dot = Vector3.Dot(goalInBoneSpace, endEffectorInBoneSpace);

            //Clamp to -1 and 1 to avoid any possible floating point precision errors
            dot = MathHelper.Clamp(dot, -1, 1); 

            //Compute the angle.
            float angle = (float)Math.Acos(dot); 
            angle = MathHelper.WrapAngle(angle);

            // We can clamp the angle here which will make the animation look smoother. 
            // However, for demonstration purposes we will comment this out for now.
            //float clampAmount = MathHelper.ToRadians(.1f);
            //angle = MathHelper.Clamp(angle, -clampAmount, clampAmount);

            // Create the rotation matrix
            Matrix rotation = Matrix.CreateFromAxisAngle(axis, angle);

            // Rotate the current bone by the new rotation matrix
            transforms[curBone] *= rotation;
        }

        /// <summary>
        /// Updates the list of world transforms for a bone hierarchy. The hierarchy 
        /// must be sorted by bone depth where the parent bone is at the head of 
        /// the list
        /// </summary>
        /// <param name="worldTransforms">The list of world transforms to update</param>
        /// <param name="rootWorldTransform">The root world transform of 
        /// the root bone</param>
        /// <param name="bindPose">The defaulft pose of the bone hierarchy</param>
        /// <param name="animationTransforms">The transform offsets from the bind 
        /// pose</param>
        /// <param name="parentBones">The list of parent bones for each bone 
        /// index.</param>
        static public void UpdateTransforms(IList<Matrix> worldTransforms, 
            IList<Matrix> localTransforms, Matrix rootWorldTransform,
            IList<Matrix> bindPose, IList<Matrix> animationTransforms,
            IList<int> parentBones)
        {
            //Set the parent bone transform
            localTransforms[0] = Matrix.Multiply( animationTransforms[0], bindPose[0]);
            worldTransforms[0] = Matrix.Multiply( localTransforms[0], 
                                                  rootWorldTransform);

            // Loop all of the bones.
            // Since the bone hierarchy is sorted by depth 
            // we will transform the parent before any child.
            for (int curBone = 1; curBone < worldTransforms.Count; curBone++)
            {
                //calculate the local transform of the bone
                Matrix local = Matrix.Multiply(animationTransforms[curBone], 
                                                    bindPose[curBone]);

                // Find the transform of this bones parent.
                // If this is the first bone use the world matrix used on the avatar
                Matrix parentMatrix = worldTransforms[parentBones[curBone]];

                // Calculate this bones world space position
                localTransforms[curBone] = local;
                worldTransforms[curBone] = Matrix.Multiply( local, parentMatrix);
            }
        }

        #endregion

        #region Draw

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);


            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            DrawCylinderChain();
            DrawAvatar();

            // Draw the cat
            Vector3 cameraPosition = Matrix.Invert(view).Translation;
            cat.Draw(cameraPosition, view, projection);

            DrawHUD();

        }

        /// <summary>
        /// Draws the platform specific HUD
        /// </summary>
        private void DrawHUD()
        {
#if XBOX
            DrawXboxSpecificHUD();
#else
            DrawWindowsSpecificHUD();
#endif
        }

        /// <summary>
        /// Render the controls on the screen
        /// </summary>
        private void DrawXboxSpecificHUD()
        {
            string pausedText = "Simulation: Running";
            if (!runSimulation)
                pausedText = "Simulation: Paused";
            pausedText += "\nPress 'A' to toggle";

            string singleStepText = "Single Step is: ON";
            if (!singleStep)
                singleStepText = "Single Step is: OFF";
            singleStepText += "\nPress 'Start' to toggle";

            string stepThrough = "";
            if (singleStep || !runSimulation)
                stepThrough = "\nPress 'B' to step once";

            string controlsText = "-Controls-\n";
            controlsText += "Move camera: Right Thumbstick\n";
            controlsText += "Zoom camera: Left/Right Trigger\n";
            controlsText += "Move cat: Left Thumbstick\n";
            controlsText += "Zoom cat: X/Y Button\n";
            controlsText += "Reset: Thumbstick Down";

            spriteBatch.Begin();
            spriteBatch.DrawString(font, pausedText, new Vector2(100, 80), Color.White);
            spriteBatch.DrawString(font, singleStepText, new Vector2(100, 120), Color.White);
            spriteBatch.DrawString(font, stepThrough, new Vector2(100, 160), Color.White);
            spriteBatch.DrawString(font, controlsText, new Vector2(100, 300), Color.White);
            spriteBatch.End();
        }

        /// <summary>
        /// Render the controls on the screen
        /// </summary>
        private void DrawWindowsSpecificHUD()
        {
            string pausedText = "Simulation: Running";
            if (!runSimulation)
                pausedText = "Simulation: Paused";
            pausedText += "\nPress 'P' to toggle";

            string singleStepText = "Single Step is: ON";
            if (!singleStep)
                singleStepText = "Single Step is: OFF";
            singleStepText += "\nPress 'Enter' to toggle";
            
            string stepThrough = "";
            if (singleStep || !runSimulation)
                stepThrough = "\nPress 'Space' to step once";

            string controlsText = "-Controls-\n";
            controlsText += "Move camera: Arrow Keys\n";
            controlsText += "Zoom camera: Z/X Key\n";
            controlsText += "Move cat: W,A,S,D Keys\n";
            controlsText += "Zoom cat: Q/E Key\n";
            controlsText += "Reset: R Key";

            spriteBatch.Begin();
            spriteBatch.DrawString(font, pausedText, new Vector2(100, 80), Color.White);
            spriteBatch.DrawString(font, singleStepText, new Vector2(100, 120), Color.White);
            spriteBatch.DrawString(font, stepThrough, new Vector2(100, 160), Color.White);
            spriteBatch.DrawString(font, controlsText, new Vector2(100, 300), Color.White);
            spriteBatch.End();
        }

        /// <summary>
        /// Draws the cylinder chain
        /// </summary>
        private void DrawCylinderChain()
        {
            Matrix modelTransform = Matrix.CreateScale(.04f, .025f, .04f);
            DrawBones(cylinderModel, modelTransform, cylinderChainIndex,
                cylinderChain, cylinderWorldTransforms);
        }

        /// <summary>
        /// Draws the Avatar and it's IK Chain
        /// </summary>
        private void DrawAvatar()
        {
            avatarRenderer.View = view;
            avatarRenderer.Projection = projection;
            avatarRenderer.Draw(avatarBoneTransforms, new AvatarExpression());

            // Draw the avatar bone chain
            Matrix S = Matrix.CreateScale(.04f, .01f, .04f);
            Matrix R = Matrix.CreateRotationZ(MathHelper.ToRadians(90));
            Matrix modelTransform = Matrix.Multiply(S, R);

            if (avatarRenderer.State == AvatarRendererState.Ready)
            {
                DrawBones(cylinderModel, modelTransform, avatarChainIndex, 
                    avatarBoneChain, avatarWorldTransforms);
            }
        }

        /// <summary>
        /// Draws the bones of a bone chain using a given model and scale
        /// </summary>
        /// <param name="model">The model to use to represent the bones</param>
        /// <param name="scale">The scale of the model when drawing the bones</param>
        /// <param name="boneToColor">The current updating bone that will appear as a 
        /// unique color. This param is only used if the user is watching each bone 
        /// update one at a time: (singStep == true)</param>
        /// <param name="rootWorldTransform">The world transform for the 
        /// root of the bone chain</param>
        /// <param name="boneChain">The bone chain to draw</param>
        /// <param name="bindPose">The bind pose of the IK objects or the default
        /// position and orientation of the bones in the IK chain</param>
        /// <param name="transforms">The list of rotational offsets from the 
        /// bind pose</param>
        /// <param name="parentBones">The list of parent bones for each 
        /// bone index.</param>
        private void DrawBones(Model model, Matrix modelTransform, int boneToColor, 
            IList<int> boneChain, IList<Matrix> worldTransfroms)
        {
            //Compute the colors we will use to render the bone chain
            Vector3 red = Color.Red.ToVector3();
            Vector3 black = Color.Black.ToVector3();
            Vector3 lightGray = Color.LightGray.ToVector3();

            // Configure the BasicEffect
            foreach (int curBone in boneChain)
            {
                // Change the color of the bone we are updating if single 
                // step is enabled
                Vector3 diffuseColor = lightGray;
                if (singleStep)
                {
                    if (boneChain[boneToColor] == curBone)
                        diffuseColor = red;
                    else if (curBone == boneChain[0])
                        diffuseColor = black;
                }

                //Draw the model
                foreach (ModelMesh mesh in model.Meshes)
                {
                    //Configure the basic effects
                    foreach (Effect effect in mesh.Effects)
	                {
                        BasicEffect basicEffect = effect as BasicEffect;
                        if(basicEffect != null)
                        {
                            basicEffect.World =
                                modelTransform * worldTransfroms[curBone];
                            basicEffect.View = view;
                            basicEffect.Projection = projection;
                            basicEffect.DiffuseColor = diffuseColor;
                        }
	                }
                    mesh.Draw();
                }
            }
        } 
        #endregion

        #region Handle Input

        /// <summary>
        /// Handle controler and keyboad input to move the cat the camera and to allow
        /// the user to exit the sample
        /// </summary>
        private void HandleInput(GameTime gameTime)
        {
            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Save current input states as the previous states for the next update.
            prevGamePadState = currentGamePadState;
            prevKeyboardState = currentKeyboardState;

            //Get the new input states
            currentGamePadState = GamePad.GetState(PlayerIndex.One);
            currentKeyboardState = Keyboard.GetState();
            
            // Allows the game to exit            
            if (IsDown(Buttons.Back) || IsDown(Keys.Escape))
                this.Exit();

            // Allow the user to reset the camera and cat.
            if (IsTriggered(Buttons.LeftStick) ||
                IsTriggered(Buttons.RightStick) ||
                IsTriggered(Keys.R))
            {
                Reset();
            }

            // Allow the user to start and stop the IK simulation
            if (IsTriggered(Buttons.A) || IsTriggered(Keys.P))
            {
                runSimulation = !runSimulation;
            }

            // If the user presses B he/she want's to pause and step through 
            // the simulation
            if (IsTriggered(Buttons.B) || IsTriggered(Keys.Space))
            {
                runSimulation = false;
            }

            // Allow the user to toggle single step mode
            if (IsTriggered(Buttons.Start) || IsTriggered(Keys.Enter))
            {
                singleStep = !singleStep;
            }

            // Handle input that will control the cat
            float moveSpeed = .1f;
            Vector2 movement = currentGamePadState.ThumbSticks.Left * moveSpeed;
            if (IsDown(Keys.W))
                movement.Y += moveSpeed;
            if (IsDown(Keys.S))
                movement.Y -= moveSpeed;
            if (IsDown(Keys.A))
                movement.X -= moveSpeed;
            if (IsDown(Keys.D))
                movement.X += moveSpeed;

            // Allow the cat to be moved toward and away from the center
            float zoom = 0;
            if (IsDown(Buttons.Y) || IsDown(Keys.Q))
                zoom = .008f;
            if (IsDown(Buttons.X) || IsDown(Keys.E))
                zoom = -.008f;

            // Update the radius and rotation angels of the cat
            movement.X = -movement.X;
            cat.Position += new Vector3(movement, zoom * time);

            // Handle input that will control the camera
            moveSpeed = .1f;
            movement = currentGamePadState.ThumbSticks.Right * moveSpeed;
            if (IsDown(Keys.Up))
                movement.Y += moveSpeed;
            if (IsDown(Keys.Down))
                movement.Y -= moveSpeed;
            if (IsDown(Keys.Left))
                movement.X -= moveSpeed;
            if (IsDown(Keys.Right))
                movement.X += moveSpeed;

            // Allow the camera to be zoomed in and out
            zoom = 0;
            zoom += currentGamePadState.Triggers.Right * .01f;
            zoom -= currentGamePadState.Triggers.Left * .01f;
            if (IsDown(Keys.Z))
                zoom = .007f;
            if (IsDown(Keys.X))
                zoom = -.007f;

            // Update the rotation angles and radius of the camera
            cameraRotX += movement.Y * time;
            cameraRotY += movement.X * time;
            cameraRadius += zoom * time;
        }

        /// <summary>
        /// Resets everything
        /// </summary>
        private void Reset()
        {
            cat.Position = new Vector3(-1, .25f, -2);
            cameraRotX = 0;
            cameraRotY = 0;
            cameraRadius = 5;
            runSimulation = true;
            singleStep = false;

            InitializeCylinderChain();
            LoadAvatar();
        }

        /// <summary>
        /// Returns if a button/key was just pressed
        /// </summary>
        /// <param name="button">The button to check</param>
        public bool IsTriggered(Buttons button)
        {
            return currentGamePadState.IsButtonDown(button) && 
                !prevGamePadState.IsButtonDown(button);
        }

        /// <summary>
        /// Returns if a button/key was just pressed
        /// </summary>
        /// <param name="key">The key to check</param>
        public bool IsTriggered(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key) &&
                !prevKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Returns if a button/key is down
        /// </summary>
        public bool IsDown(Buttons button)
        {
            return currentGamePadState.IsButtonDown(button);
        }

        /// <summary>
        /// Returns if a button/key is down
        /// </summary>
        public bool IsDown(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key);
        }
        #endregion
    }

    #region Entry Point
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (IKSample game = new IKSample())
            {
                game.Run();
            }
        }
    } 
    #endregion
}
