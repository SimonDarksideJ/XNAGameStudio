#region File Description
//-----------------------------------------------------------------------------
// InstancedModelSampleGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace InstancedModelSample
{
    /// <summary>
    /// Enum describes the various possible techniques
    /// that can be chosen to implement instancing.
    /// </summary>
    public enum InstancingTechnique
    {
        HardwareInstancing,
        NoInstancing,
        NoInstancingOrStateBatching
    }


    /// <summary>
    /// Sample showing how to efficiently render many copies of a model, using
    /// hardware instancing to draw more than one copy in a single GPU batch.
    /// </summary>
    public class InstancedModelSampleGame : Microsoft.Xna.Framework.Game
    {
        #region Fields


        GraphicsDeviceManager graphics;

        SpriteBatch spriteBatch;
        SpriteFont spriteFont;


        // Instanced model rendering.
        InstancingTechnique instancingTechnique = InstancingTechnique.HardwareInstancing;

        const int InitialInstanceCount = 1000;

        List<SpinningInstance> instances;
        Matrix[] instanceTransforms;
        Model instancedModel;
        Matrix[] instancedModelBones;
        DynamicVertexBuffer instanceVertexBuffer;


        // To store instance transform matrices in a vertex buffer, we use this custom
        // vertex type which encodes 4x4 matrices as a set of four Vector4 values.
        static VertexDeclaration instanceVertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0,  VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
            new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
            new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
            new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3)
        );


        // Measure the framerate.
        int frameRate;
        int frameCounter;
        TimeSpan elapsedTime;


        // Input handling.
        KeyboardState lastKeyboardState;
        GamePadState lastGamePadState;
        KeyboardState currentKeyboardState;
        GamePadState currentGamePadState;


        #endregion

        #region Initialization


        public InstancedModelSampleGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Most games will want to leave both these values set to true to ensure
            // smoother updates, but when you are doing performance work it can be
            // useful to set them to false in order to get more accurate measurements.
            IsFixedTimeStep = false;

            graphics.SynchronizeWithVerticalRetrace = false;

            // Initialize the list of instances.
            instances = new List<SpinningInstance>();

            for (int i = 0; i < InitialInstanceCount; i++)
                instances.Add(new SpinningInstance());
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            
            spriteFont = Content.Load<SpriteFont>("Font");

            instancedModel = Content.Load<Model>("Cats");
            instancedModelBones = new Matrix[instancedModel.Bones.Count];
            instancedModel.CopyAbsoluteBoneTransformsTo(instancedModelBones);
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            HandleInput();

            // Update the position of each spinning instance.
            foreach (SpinningInstance instance in instances)
            {
                instance.Update(gameTime);
            }

            // Measure our framerate.
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            } 
            
            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice device = graphics.GraphicsDevice;

            device.Clear(Color.CornflowerBlue);

            // Calculate camera matrices.
            Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 15),
                                              Vector3.Zero, Vector3.Up);

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                    device.Viewport.AspectRatio,
                                                                    1, 
                                                                    100);

            // Set renderstates for drawing 3D models.
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;

            // Gather instance transform matrices into a single array.
            Array.Resize(ref instanceTransforms, instances.Count);

            for (int i = 0; i < instances.Count; i++)
            {
                instanceTransforms[i] = instances[i].Transform;
            }

            // Draw all the instances, using the currently selected rendering technique.
            switch (instancingTechnique)
            {
                case InstancingTechnique.HardwareInstancing:
                    DrawModelHardwareInstancing(instancedModel, instancedModelBones,
                                                instanceTransforms, view, projection);
                    break;

                case InstancingTechnique.NoInstancing:
                    DrawModelNoInstancing(instancedModel, instancedModelBones,
                                          instanceTransforms, view, projection);
                    break;

                case InstancingTechnique.NoInstancingOrStateBatching:
                    DrawModelNoInstancingOrStateBatching(instancedModel, instancedModelBones,
                                                         instanceTransforms, view, projection);
                    break;
            }

            DrawOverlayText();
            
            // Measure our framerate.
            frameCounter++;

            base.Draw(gameTime);
        }


        /// <summary>
        /// Efficiently draws several copies of a piece of geometry using hardware instancing.
        /// </summary>
        void DrawModelHardwareInstancing(Model model, Matrix[] modelBones,
                                         Matrix[] instances, Matrix view, Matrix projection)
        {
            if (instances.Length == 0)
                return;

            // If we have more instances than room in our vertex buffer, grow it to the neccessary size.
            if ((instanceVertexBuffer == null) ||
                (instances.Length > instanceVertexBuffer.VertexCount))
            {
                if (instanceVertexBuffer != null)
                    instanceVertexBuffer.Dispose();

                instanceVertexBuffer = new DynamicVertexBuffer(GraphicsDevice, instanceVertexDeclaration,
                                                               instances.Length, BufferUsage.WriteOnly);
            }

            // Transfer the latest instance transform matrices into the instanceVertexBuffer.
            instanceVertexBuffer.SetData(instances, 0, instances.Length, SetDataOptions.Discard);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // Tell the GPU to read from both the model vertex buffer plus our instanceVertexBuffer.
                    GraphicsDevice.SetVertexBuffers(
                        new VertexBufferBinding(meshPart.VertexBuffer, meshPart.VertexOffset, 0),
                        new VertexBufferBinding(instanceVertexBuffer, 0, 1)
                    );

                    GraphicsDevice.Indices = meshPart.IndexBuffer;

                    // Set up the instance rendering effect.
                    Effect effect = meshPart.Effect;

                    effect.CurrentTechnique = effect.Techniques["HardwareInstancing"];

                    effect.Parameters["World"].SetValue(modelBones[mesh.ParentBone.Index]);
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);

                    // Draw all the instance copies in a single call.
                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                                               meshPart.NumVertices, meshPart.StartIndex,
                                                               meshPart.PrimitiveCount, instances.Length);
                    }
                }
            }
        }


        /// <summary>
        /// Draws several copies of a piece of geometry without using any
        /// special GPU instancing techniques at all. This just does a
        /// regular loop and issues several draw calls one after another.
        /// </summary>
        void DrawModelNoInstancing(Model model, Matrix[] modelBones,
                                   Matrix[] instances, Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer, meshPart.VertexOffset);
                    GraphicsDevice.Indices = meshPart.IndexBuffer;

                    // Set up the rendering effect.
                    Effect effect = meshPart.Effect;

                    effect.CurrentTechnique = effect.Techniques["NoInstancing"];

                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);

                    EffectParameter transformParameter = effect.Parameters["World"];

                    // Draw a single instance copy each time around this loop.
                    for (int i = 0; i < instances.Length; i++)
                    {
                        transformParameter.SetValue(modelBones[mesh.ParentBone.Index] * instances[i]);

                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();

                            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                                                 meshPart.NumVertices, meshPart.StartIndex,
                                                                 meshPart.PrimitiveCount);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// This technique is NOT a good idea! It is only included in the sample
        /// for comparison purposes, so you can compare its performance with the
        /// other more sensible approaches. This uses the exact same shader code
        /// as the preceding NoInstancing technique, but with a key difference.
        /// Where the NoInstancing technique worked like this:
        /// 
        ///     SetRenderStates()
        ///     foreach instance
        ///     {
        ///         Update effect with per-instance transform matrix
        ///         DrawIndexedPrimitives()
        ///     }
        /// 
        /// NoInstancingOrStateBatching works like so:
        /// 
        ///     foreach instance
        ///     {
        ///         Set per-instance transform matrix into the effect
        ///         SetRenderStates()
        ///         DrawIndexedPrimitives()
        ///     }
        ///      
        /// As you can see, this is repeatedly setting the same renderstates.
        /// Not so efficient.
        /// 
        /// In other words, the built-in Model.Draw method is pretty inefficient when
        /// it comes to drawing more than one instance! Even without using any fancy
        /// shader techniques, you can get a significant speed boost just by rearranging
        /// your drawing code to work more like the earlier NoInstancing technique.
        /// </summary>
        void DrawModelNoInstancingOrStateBatching(Model model, Matrix[] modelBones,
                                                  Matrix[] instances, Matrix view, Matrix projection)
        {
            for (int i = 0; i < instances.Length; i++)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (Effect effect in mesh.Effects)
                    {
                        effect.CurrentTechnique = effect.Techniques["NoInstancing"];

                        effect.Parameters["World"].SetValue(modelBones[mesh.ParentBone.Index] * instances[i]);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);
                    }

                    mesh.Draw();
                }
            }
        }


        /// <summary>
        /// Helper for drawing the help text overlay.
        /// </summary>
        void DrawOverlayText()
        {
            string text = string.Format(CultureInfo.CurrentCulture,
                                        "Frames per second: {0}\n" +
                                        "Instances: {1}\n" +
                                        "Technique: {2}\n\n" +
                                        "A = Change technique\n" +
                                        "X = Add instances\n" +
                                        "Y = Remove instances\n",
                                        frameRate,
                                        instances.Count,
                                        instancingTechnique);

            spriteBatch.Begin();

            spriteBatch.DrawString(spriteFont, text, new Vector2(65, 65), Color.Black);
            spriteBatch.DrawString(spriteFont, text, new Vector2(64, 64), Color.White);

            spriteBatch.End();
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Handles input for quitting or changing settings.
        /// </summary>
        void HandleInput()
        {
            lastKeyboardState = currentKeyboardState;
            lastGamePadState = currentGamePadState;

            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Check for exit.
            if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
                currentGamePadState.Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }

            // Change the number of instances more quickly if there are
            // already lots of them. This avoids you having to sit there
            // for hours with your finger on the "increase" button!
            int instanceChangeRate = Math.Max(instances.Count / 100, 1);

            // Increase the number of instances?
            if (currentKeyboardState.IsKeyDown(Keys.X) ||
                currentGamePadState.Buttons.X == ButtonState.Pressed)
            {
                for (int i = 0; i < instanceChangeRate; i++)
                {
                    instances.Add(new SpinningInstance());
                }
            }

            // Decrease the number of instances?
            if (currentKeyboardState.IsKeyDown(Keys.Y) ||
                currentGamePadState.Buttons.Y == ButtonState.Pressed)
            {
                for (int i = 0; i < instanceChangeRate; i++)
                {
                    if (instances.Count == 0)
                        break;

                    instances.RemoveAt(instances.Count - 1);
                }
            }

            // Change which instancing technique we are using?
            if ((currentKeyboardState.IsKeyDown(Keys.A) &&
                 lastKeyboardState.IsKeyUp(Keys.A)) ||
                (currentGamePadState.Buttons.A == ButtonState.Pressed &&
                 lastGamePadState.Buttons.A == ButtonState.Released))
            {
                instancingTechnique++;

                // Wrap if we reach the end of the possible techniques.
                if (instancingTechnique > InstancingTechnique.NoInstancingOrStateBatching)
                    instancingTechnique = 0;
            }
        }


        #endregion
    }


    #region Entry Point

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static class Program
    {
        static void Main()
        {
            using (InstancedModelSampleGame game = new InstancedModelSampleGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
