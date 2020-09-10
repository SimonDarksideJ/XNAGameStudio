#region File Description
//-----------------------------------------------------------------------------
// Game.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace TrianglePicking
{
    /// <summary>
    /// Sample showing how to implement per-triangle picking. This uses a custom
    /// content pipeline processor to attach a list of vertex position data to each
    /// model as part of the build process, and then implements a ray-to-triangle
    /// intersection method to collide against this vertex data.
    /// </summary>
    public class TrianglePickingGame : Microsoft.Xna.Framework.Game
    {
        #region Constants

        // ModelFilenames is the list of models that we will be putting on top of the
        // table. These strings will be used as arguments to content.Load<Model> and
        // will be drawn when the cursor is over an object.
        static readonly string[] ModelFilenames =
        {
            "Sphere",
            "Cats",
            "P2Wedge",
            "Cylinder",
        };
        
        // the following constants control the speed at which the camera moves
        // how fast does the camera move up, down, left, and right?
        const float CameraRotateSpeed = .1f;
        // how fast does the camera zoom in and out?
        const float CameraZoomSpeed = .01f;
        // the camera can't be further away than this distance
        const float CameraMaxDistance = 10.0f;
        // and it can't be closer than this
        const float CameraMinDistance = 1.2f;

        // the following constants control how the camera's default position
        const float CameraDefaultArc = -30.0f;
        const float CameraDefaultRotation = 225;
        const float CameraDefaultDistance = 3.5f;

        #endregion

        #region Fields

        GraphicsDeviceManager graphics;

        // the current input states.  These are updated in the HandleInput function,
        // and used primarily in the UpdateCamera function.
        KeyboardState currentKeyboardState;
        GamePadState currentGamePadState;

        // a SpriteBatch and SpriteFont, which we will use to draw the objects' names
        // when they are selected.
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        // The cursor is used to tell what the user's pointer/mouse is over. The cursor
        // is moved with the left thumbstick. On windows, the mouse can be used as well.
        Cursor cursor;

        // the table that all of the objects are drawn on, and table model's 
        // absoluteBoneTransforms. Since the table is not animated, these can be 
        // calculated once and saved.
        Model table;
        Matrix[] tableAbsoluteBoneTransforms;

        // these are the models that we will draw on top of the table. we'll store them
        // and their bone transforms in arrays. Again, since these models aren't
        // animated, we can calculate their bone transforms once and save the result.
        Model[] models = new Model[ModelFilenames.Length];
        Matrix[][] modelAbsoluteBoneTransforms = new Matrix[ModelFilenames.Length][];
        // each model will need one more matrix: a world transform. This matrix will be
        // used to place each model at a different location in the world.
        Matrix[] modelWorldTransforms = new Matrix[ModelFilenames.Length];

        // The next set of variables are used to control the camera used in the sample. 
        // It is an arc ball camera, so it can rotate in a sphere around the target, and
        // zoom in and out.
        float cameraArc = CameraDefaultArc;
        float cameraRotation = CameraDefaultRotation;
        float cameraDistance = CameraDefaultDistance;
        Matrix viewMatrix;
        Matrix projectionMatrix;

        // To keep things efficient, the picking works by first applying a bounding
        // sphere test, and then only bothering to test each individual triangle
        // if the ray intersects the bounding sphere. This allows us to trivially
        // reject many models without even needing to bother looking at their triangle
        // data. This field keeps track of which models passed the bounding sphere
        // test, so you can see the difference between this approximation and the more
        // accurate triangle picking.
        List<string> insideBoundingSpheres = new List<string>();

        // Store the name of the model underneath the cursor (or null if there is none).
        string pickedModelName;

        // Vertex array that stores exactly which triangle was picked.
        VertexPositionColor[] pickedTriangle =
        {
            new VertexPositionColor(Vector3.Zero, Color.Magenta),
            new VertexPositionColor(Vector3.Zero, Color.Magenta),
            new VertexPositionColor(Vector3.Zero, Color.Magenta),
        };

        // Effect and vertex declaration for drawing the picked triangle.
        BasicEffect lineEffect;

        // Custom rasterizer state for drawing in wireframe.
        static RasterizerState WireFrame = new RasterizerState
        {
            FillMode = FillMode.WireFrame,
            CullMode = CullMode.None
        };

        #endregion

        #region Initialization


        public TrianglePickingGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Set up the world transforms that each model will use. They'll be
            // positioned in a line along the x axis.
            for (int i = 0; i < modelWorldTransforms.Length; i++)
            {
                float x = i - modelWorldTransforms.Length / 2;
                modelWorldTransforms[i] =
                    Matrix.CreateTranslation(new Vector3(x, 0, 0));
            }

            cursor = new Cursor(this, Content);
            Components.Add(cursor);
        }


        protected override void Initialize()
        {
            // now that the GraphicsDevice has been created, we can create the projection matrix.
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f), GraphicsDevice.Viewport.AspectRatio, .01f, 1000);

            base.Initialize();
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            // load all of the models that will appear on the table:
            for (int i = 0; i < ModelFilenames.Length; i++)
            {
                // load the actual model, using ModelFilenames to determine what
                // file to load.
                models[i] = Content.Load<Model>(ModelFilenames[i]);

                // create an array of matrices to hold the absolute bone transforms,
                // calculate them, and copy them in.
                modelAbsoluteBoneTransforms[i] = new Matrix[models[i].Bones.Count];
                models[i].CopyAbsoluteBoneTransformsTo(
                    modelAbsoluteBoneTransforms[i]);
            }

            // now that we've loaded in the models that will sit on the table, go
            // through the same procedure for the table itself.
            table = Content.Load<Model>("Table");
            tableAbsoluteBoneTransforms = new Matrix[table.Bones.Count];
            table.CopyAbsoluteBoneTransformsTo(tableAbsoluteBoneTransforms);

            // create a spritebatch and load the font, which we'll use to draw the
            // models' names.
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("hudFont");

            // create the effect and vertex declaration for drawing the
            // picked triangle.
            lineEffect = new BasicEffect(graphics.GraphicsDevice);
            lineEffect.VertexColorEnabled = true;
        }


        #endregion

        #region Update and Draw
    

        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            HandleInput();

            UpdateCamera(gameTime);

            UpdatePicking();

            base.Update(gameTime);
        }


        /// <summary>
        /// Runs a per-triangle picking algorithm over all the models in the scene,
        /// storing which triangle is currently under the cursor.
        /// </summary>
        void UpdatePicking()
        {
            // Look up a collision ray based on the current cursor position. See the
            // Picking Sample documentation for a detailed explanation of this.
            Ray cursorRay = cursor.CalculateCursorRay(projectionMatrix, viewMatrix);

            // Clear the previous picking results.
            insideBoundingSpheres.Clear();

            pickedModelName = null;
            
            // Keep track of the closest object we have seen so far, so we can
            // choose the closest one if there are several models under the cursor.
            float closestIntersection = float.MaxValue;

            // Loop over all our models.
            for (int i = 0; i < models.Length; i++)
            {
                bool insideBoundingSphere;
                Vector3 vertex1, vertex2, vertex3;

                // Perform the ray to model intersection test.
                float? intersection = RayIntersectsModel(cursorRay, models[i],
                                                         modelWorldTransforms[i],
                                                         out insideBoundingSphere,
                                                         out vertex1, out vertex2,
                                                         out vertex3);

                // If this model passed the initial bounding sphere test, remember
                // that so we can display it at the top of the screen.
                if (insideBoundingSphere)
                    insideBoundingSpheres.Add(ModelFilenames[i]);

                // Do we have a per-triangle intersection with this model?
                if (intersection != null)
                {
                    // If so, is it closer than any other model we might have
                    // previously intersected?
                    if (intersection < closestIntersection)
                    {
                        // Store information about this model.
                        closestIntersection = intersection.Value;

                        pickedModelName = ModelFilenames[i];

                        // Store vertex positions so we can display the picked triangle.
                        pickedTriangle[0].Position = vertex1;
                        pickedTriangle[1].Position = vertex2;
                        pickedTriangle[2].Position = vertex3;
                    }
                }
            }
        }


        /// <summary>
        /// Checks whether a ray intersects a model. This method needs to access
        /// the model vertex data, so the model must have been built using the
        /// custom TrianglePickingProcessor provided as part of this sample.
        /// Returns the distance along the ray to the point of intersection, or null
        /// if there is no intersection.
        /// </summary>
        static float? RayIntersectsModel(Ray ray, Model model, Matrix modelTransform,
                                         out bool insideBoundingSphere,
                                         out Vector3 vertex1, out Vector3 vertex2,
                                         out Vector3 vertex3)
        {
            vertex1 = vertex2 = vertex3 = Vector3.Zero;

            // The input ray is in world space, but our model data is stored in object
            // space. We would normally have to transform all the model data by the
            // modelTransform matrix, moving it into world space before we test it
            // against the ray. That transform can be slow if there are a lot of
            // triangles in the model, however, so instead we do the opposite.
            // Transforming our ray by the inverse modelTransform moves it into object
            // space, where we can test it directly against our model data. Since there
            // is only one ray but typically many triangles, doing things this way
            // around can be much faster.

            Matrix inverseTransform = Matrix.Invert(modelTransform);

            ray.Position = Vector3.Transform(ray.Position, inverseTransform);
            ray.Direction = Vector3.TransformNormal(ray.Direction, inverseTransform);

            // Look up our custom collision data from the Tag property of the model.
            Dictionary<string, object> tagData = (Dictionary<string, object>)model.Tag;

            if (tagData == null)
            {
                throw new InvalidOperationException(
                    "Model.Tag is not set correctly. Make sure your model " +
                    "was built using the custom TrianglePickingProcessor.");
            }

            // Start off with a fast bounding sphere test.
            BoundingSphere boundingSphere = (BoundingSphere)tagData["BoundingSphere"];

            if (boundingSphere.Intersects(ray) == null)
            {
                // If the ray does not intersect the bounding sphere, we cannot
                // possibly have picked this model, so there is no need to even
                // bother looking at the individual triangle data.
                insideBoundingSphere = false;

                return null;
            }
            else
            {
                // The bounding sphere test passed, so we need to do a full
                // triangle picking test.
                insideBoundingSphere = true;

                // Keep track of the closest triangle we found so far,
                // so we can always return the closest one.
                float? closestIntersection = null;

                // Loop over the vertex data, 3 at a time (3 vertices = 1 triangle).
                Vector3[] vertices = (Vector3[])tagData["Vertices"];

                for (int i = 0; i < vertices.Length; i += 3)
                {
                    // Perform a ray to triangle intersection test.
                    float? intersection;

                    RayIntersectsTriangle(ref ray,
                                          ref vertices[i],
                                          ref vertices[i + 1],
                                          ref vertices[i + 2],
                                          out intersection);

                    // Does the ray intersect this triangle?
                    if (intersection != null)
                    {
                        // If so, is it closer than any other previous triangle?
                        if ((closestIntersection == null) ||
                            (intersection < closestIntersection))
                        {
                            // Store the distance to this triangle.
                            closestIntersection = intersection;

                            // Transform the three vertex positions into world space,
                            // and store them into the output vertex parameters.
                            Vector3.Transform(ref vertices[i],
                                              ref modelTransform, out vertex1);
                            
                            Vector3.Transform(ref vertices[i + 1],
                                              ref modelTransform, out vertex2);
                            
                            Vector3.Transform(ref vertices[i + 2],
                                              ref modelTransform, out vertex3);
                        }
                    }
                }

                return closestIntersection;
            }
        }


        /// <summary>
        /// Checks whether a ray intersects a triangle. This uses the algorithm
        /// developed by Tomas Moller and Ben Trumbore, which was published in the
        /// Journal of Graphics Tools, volume 2, "Fast, Minimum Storage Ray-Triangle
        /// Intersection".
        /// 
        /// This method is implemented using the pass-by-reference versions of the
        /// XNA math functions. Using these overloads is generally not recommended,
        /// because they make the code less readable than the normal pass-by-value
        /// versions. This method can be called very frequently in a tight inner loop,
        /// however, so in this particular case the performance benefits from passing
        /// everything by reference outweigh the loss of readability.
        /// </summary>
        static void RayIntersectsTriangle(ref Ray ray,
                                          ref Vector3 vertex1,
                                          ref Vector3 vertex2,
                                          ref Vector3 vertex3, out float? result)
        {
            // Compute vectors along two edges of the triangle.
            Vector3 edge1, edge2;
            
            Vector3.Subtract(ref vertex2, ref vertex1, out edge1);
            Vector3.Subtract(ref vertex3, ref vertex1, out edge2);

            // Compute the determinant.
            Vector3 directionCrossEdge2;
            Vector3.Cross(ref ray.Direction, ref edge2, out directionCrossEdge2);

            float determinant;
            Vector3.Dot(ref edge1, ref directionCrossEdge2, out determinant);

            // If the ray is parallel to the triangle plane, there is no collision.
            if (determinant > -float.Epsilon && determinant < float.Epsilon)
            {
                result = null;
                return;
            }

            float inverseDeterminant = 1.0f / determinant;

            // Calculate the U parameter of the intersection point.
            Vector3 distanceVector;
            Vector3.Subtract(ref ray.Position, ref vertex1, out distanceVector);

            float triangleU;
            Vector3.Dot(ref distanceVector, ref directionCrossEdge2, out triangleU);
            triangleU *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleU < 0 || triangleU > 1)
            {
                result = null;
                return;
            }

            // Calculate the V parameter of the intersection point.
            Vector3 distanceCrossEdge1;
            Vector3.Cross(ref distanceVector, ref edge1, out distanceCrossEdge1);

            float triangleV;
            Vector3.Dot(ref ray.Direction, ref distanceCrossEdge1, out triangleV);
            triangleV *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleV < 0 || triangleU + triangleV > 1)
            {
                result = null;
                return;
            }

            // Compute the distance along the ray to the triangle.
            float rayDistance;
            Vector3.Dot(ref edge2, ref distanceCrossEdge1, out rayDistance);
            rayDistance *= inverseDeterminant;

            // Is the triangle behind the ray origin?
            if (rayDistance < 0)
            {
                result = null;
                return;
            }

            result = rayDistance;
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice device = graphics.GraphicsDevice;

            device.Clear(Color.CornflowerBlue);

            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;

            // Draw the table.
            DrawModel(table, Matrix.Identity, tableAbsoluteBoneTransforms);

            // Use the same DrawModel function to draw all of the models on the table.
            for (int i = 0; i < models.Length; i++)
            {
                DrawModel(models[i], modelWorldTransforms[i],
                                     modelAbsoluteBoneTransforms[i]);
            }

            // Draw the outline of the triangle under the cursor.
            DrawPickedTriangle();

            // Draw text describing the picking results.
            DrawText();

            base.Draw(gameTime);
        }


        /// <summary>
        /// Helper for drawing the outline of the triangle currently under the cursor.
        /// </summary>
        void DrawPickedTriangle()
        {
            if (pickedModelName != null)
            {
                GraphicsDevice device = graphics.GraphicsDevice;

                // Set line drawing renderstates. We disable backface culling
                // and turn off the depth buffer because we want to be able to
                // see the picked triangle outline regardless of which way it is
                // facing, and even if there is other geometry in front of it.
                device.RasterizerState = WireFrame;
                device.DepthStencilState = DepthStencilState.None;

                // Activate the line drawing BasicEffect.
                lineEffect.Projection = projectionMatrix;
                lineEffect.View = viewMatrix;

                lineEffect.CurrentTechnique.Passes[0].Apply();

                // Draw the triangle.
                device.DrawUserPrimitives(PrimitiveType.TriangleList,
                                          pickedTriangle, 0, 1);

                // Reset renderstates to their default values.
                device.RasterizerState = RasterizerState.CullCounterClockwise;
                device.DepthStencilState = DepthStencilState.Default;
            }
        }


        /// <summary>
        /// Helper for drawing text showing the current picking results.
        /// </summary>
        void DrawText()
        {
            // Draw the text twice to create a drop-shadow effect, first in black one
            // pixel down and to the right, then again in white at the real position.
            Vector2 shadowOffset = new Vector2(1, 1);

            spriteBatch.Begin();

            // Draw a list of which models passed the initial bounding sphere test.
            if (insideBoundingSpheres.Count > 0)
            {
                string text = "Inside bounding sphere: " +
                                string.Join(", ", insideBoundingSpheres.ToArray());

                Vector2 position = new Vector2(50, 50);

                spriteBatch.DrawString(spriteFont, text,
                                       position + shadowOffset, Color.Black);
                
                spriteBatch.DrawString(spriteFont, text,
                                       position, Color.White);
            }

            // Draw the name of the model that passed the per-triangle picking test.
            if (pickedModelName != null)
            {
                Vector2 position = cursor.Position;

                // Draw the text below the cursor position.
                position.Y += 32;

                // Center the string.
                position -= spriteFont.MeasureString(pickedModelName) / 2;
                
                spriteBatch.DrawString(spriteFont, pickedModelName,
                                       position + shadowOffset, Color.Black);
                
                spriteBatch.DrawString(spriteFont, pickedModelName,
                                       position, Color.White);
            }

            spriteBatch.End();
        }


        /// <summary>
        /// DrawModel is a helper function that takes a model, world matrix, and
        /// bone transforms. It does just what its name implies, and draws the model.
        /// </summary>
        private void DrawModel(Model model, Matrix worldTransform,
                                            Matrix[] absoluteBoneTransforms)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;

                    effect.View = viewMatrix;
                    effect.Projection = projectionMatrix;
                    effect.World = absoluteBoneTransforms[mesh.ParentBone.Index] *
                                                                        worldTransform;
                }

                mesh.Draw();
            }
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Handles input for quitting the game.
        /// </summary>
        void HandleInput()
        {
            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Check for exit.
            if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
                currentGamePadState.Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }
        }


        /// <summary>
        /// Handles input for moving the camera.
        /// </summary>
        void UpdateCamera(GameTime gameTime)
        {
            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // should we reset the camera?
            if (currentKeyboardState.IsKeyDown(Keys.R) ||
                currentGamePadState.Buttons.RightStick == ButtonState.Pressed)
            {
                cameraArc = CameraDefaultArc;
                cameraDistance = CameraDefaultDistance;
                cameraRotation = CameraDefaultRotation;
            }

            // Check for input to rotate the camera up and down around the model.
            if (currentKeyboardState.IsKeyDown(Keys.Up) ||
                currentKeyboardState.IsKeyDown(Keys.W))
            {
                cameraArc += time * CameraRotateSpeed;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Down) ||
                currentKeyboardState.IsKeyDown(Keys.S))
            {
                cameraArc -= time * CameraRotateSpeed;
            }

            cameraArc += currentGamePadState.ThumbSticks.Right.Y * time *
                CameraRotateSpeed;

            // Limit the arc movement.
            cameraArc = MathHelper.Clamp(cameraArc, -90.0f, 90.0f);

            // Check for input to rotate the camera around the model.
            if (currentKeyboardState.IsKeyDown(Keys.Right) ||
                currentKeyboardState.IsKeyDown(Keys.D))
            {
                cameraRotation += time * CameraRotateSpeed;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Left) ||
                currentKeyboardState.IsKeyDown(Keys.A))
            {
                cameraRotation -= time * CameraRotateSpeed;
            }

            cameraRotation += currentGamePadState.ThumbSticks.Right.X * time *
                CameraRotateSpeed;

            // Check for input to zoom camera in and out.
            if (currentKeyboardState.IsKeyDown(Keys.Z))
                cameraDistance += time * CameraZoomSpeed;

            if (currentKeyboardState.IsKeyDown(Keys.X))
                cameraDistance -= time * CameraZoomSpeed;

            cameraDistance += currentGamePadState.Triggers.Left * time
                * CameraZoomSpeed;
            cameraDistance -= currentGamePadState.Triggers.Right * time
                * CameraZoomSpeed;

            // clamp the camera distance so it doesn't get too close or too far away.
            cameraDistance = MathHelper.Clamp(cameraDistance,
                CameraMinDistance, CameraMaxDistance);

            Matrix unrotatedView = Matrix.CreateLookAt(
                new Vector3(0, 0, -cameraDistance), Vector3.Zero, Vector3.Up);

            viewMatrix = Matrix.CreateRotationY(MathHelper.ToRadians(cameraRotation)) *
                          Matrix.CreateRotationX(MathHelper.ToRadians(cameraArc)) *
                          unrotatedView;
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
            using (TrianglePickingGame game = new TrianglePickingGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
