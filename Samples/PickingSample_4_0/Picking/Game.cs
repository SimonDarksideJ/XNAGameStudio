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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using BoundingVolumeRendering;
#endregion

namespace PickingSample
{
    /// <summary>
    /// This sample shows how to see if a user's cursor is over an object, and how to
    /// find out where on the screen an object is. The sample puts several objects on a
    /// table. If the cursor is on the object, the object's name is displayed. See the
    /// accompanying doc file for more information.
    /// </summary>
    public class PickingSampleGame : Microsoft.Xna.Framework.Game
    {
        #region Constants

        // ModelFilenames is the list of models that we will be putting on top of the
        // table. These strings will be used as arguments to content.Load<Model> and
        // will be drawn when the cursor is over an object.
        static readonly string[] ModelFilenames = new string[]{
            "Sphere",
            "Cats",
            "P2Wedge",
            "Cylinder",
        };

        // the following constants control the speed at which the camera moves
        // how fast does the camera move up, down, left, and right?
        const float CameraRotateSpeed = .01f;

        // the following constants control how the camera's default position
        const float CameraDefaultArc = -15.0f;
        const float CameraDefaultRotation = 185;
        const float CameraDefaultDistance = 4.3f;

        #endregion

        #region Fields

        GraphicsDeviceManager graphics;

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

        Matrix viewMatrix;
        Matrix projectionMatrix;

        // this variable will store the current rotation value as the camera
        // rotates around the scene
        float cameraRotation = CameraDefaultRotation;

        // this variable will tell our game whether or not to draw a mesh's bounding sphere
        bool drawBoundingSphere = true;
        #endregion

        #region Initialization

        public PickingSampleGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if WINDOWS_PHONE
            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);
            graphics.IsFullScreen = true;
#endif
        }

        protected override void Initialize()
        {
            // Set up the world transforms that each model will use. They'll be
            // positioned in a line along the x axis.
            modelWorldTransforms[0] = Matrix.CreateTranslation(new Vector3(-1.5f, 0, 0));
            modelWorldTransforms[1] = Matrix.CreateTranslation(new Vector3(-.5f, 0, 0));
            modelWorldTransforms[2] = Matrix.CreateTranslation(new Vector3(.5f, 0, 0));
            modelWorldTransforms[3] = Matrix.CreateTranslation(new Vector3(1.5f, 0, 0));

            cursor = new Cursor(this);
            Components.Add(cursor);

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

            // calculate the projection matrix now that the graphics device is created.
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, .01f, 1000);
                
            // Initialize the renderer for our bounding spheres
            BoundingSphereRenderer.Initialize(GraphicsDevice, 45);
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Check for exit.
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) ||
                GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }

            // we rotate our view around the models over time
            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            cameraRotation += time * CameraRotateSpeed;
            Matrix unrotatedView = Matrix.CreateLookAt(
                new Vector3(0, 0, -CameraDefaultDistance), Vector3.Zero, Vector3.Up);
            viewMatrix = Matrix.CreateRotationY(MathHelper.ToRadians(cameraRotation)) *
                          Matrix.CreateRotationX(MathHelper.ToRadians(CameraDefaultArc)) *
                          unrotatedView;

            // base.Update will update all of the components in the .Components
            // collection, including the cursor.
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // drawing sprites changes some render states around, which don't play
            // nicely with 3d models. In particular, we want to enable the depth buffer and turn off alpha blending.
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            // draw the table. DrawModel is a function defined below draws a model using
            // a world matrix and the model's bone transforms.
            DrawModel(table, Matrix.Identity, tableAbsoluteBoneTransforms, false);

            // use the same DrawModel function to draw all of the models on the table.
            for (int i = 0; i < models.Length; i++)
            {
                DrawModel(models[i], modelWorldTransforms[i],
                    modelAbsoluteBoneTransforms[i], drawBoundingSphere);
            }

            // now we'll check to see if the cursor is over any of the models, and draw
            // their names if it is.
            DrawModelNames();

            base.Draw(gameTime);
        }

        private void DrawModelNames()
        {
            // begin on the spritebatch, because we're going to be drawing some text.
            spriteBatch.Begin();

            // If the cursor is over a model, we'll draw its name. To figure out if
            // the cursor is over a model, we'll use cursor.CalculateCursorRay. That
            // function gives us a world space ray that starts at the "eye" of the
            // camera, and shoots out in the direction pointed to by the cursor.
            Ray cursorRay = cursor.CalculateCursorRay(projectionMatrix, viewMatrix);

            // go through all of the models...
            for (int i = 0; i < models.Length; i++)
            {
                // check to see if the cursorRay intersects the model....
                if (RayIntersectsModel(cursorRay, models[i], modelWorldTransforms[i],
                    modelAbsoluteBoneTransforms[i]))
                {

                    // now we know that we want to draw the model's name. We want to
                    // draw the name a little bit above the model: but where's that?
                    // SpriteBatch.DrawString takes screen space coordinates, but the 
                    // model's position is stored in world space. 

                    // we'll use Viewport.Project, which will project a world space
                    // point into screen space. We'll project the vector (0,0,0) using
                    // the model's world matrix, and the view and projection matrices.
                    // that will tell us where the model's origin is on the screen.
                    Vector3 screenSpace = graphics.GraphicsDevice.Viewport.Project(
                        Vector3.Zero, projectionMatrix, viewMatrix,
                        modelWorldTransforms[i]);

                    // we want to draw the text a little bit above that, so we'll use
                    // the screen space position - 60 to move up a little bit. A better
                    // approach would be to calculate where the top of the model is, and
                    // draw there. It's not that much harder to do, but to keep the
                    // sample easy, we'll take the easy way out.
                    Vector2 textPosition =
                        new Vector2(screenSpace.X, screenSpace.Y - 60);

                    // we want to draw the text centered around textPosition, so we'll
                    // calculate the center of the string, and use that as the origin
                    // argument to spriteBatch.DrawString. DrawString automatically
                    // centers text around the vector specified by the origin argument.
                    Vector2 stringCenter =
                        spriteFont.MeasureString(ModelFilenames[i]) / 2;

                    // to make the text readable, we'll draw the same thing twice, once
                    // white and once black, with a little offset to get a drop shadow
                    // effect.

                    // first we'll draw the shadow...
                    Vector2 shadowOffset = new Vector2(1, 1);
                    spriteBatch.DrawString(spriteFont, ModelFilenames[i],
                        textPosition + shadowOffset, Color.Black, 0.0f,
                        stringCenter, 1.0f, SpriteEffects.None, 0.0f);

                    // ...and then the real text on top.
                    spriteBatch.DrawString(spriteFont, ModelFilenames[i],
                        textPosition, Color.White, 0.0f,
                        stringCenter, 1.0f, SpriteEffects.None, 0.0f);
                }
            }

            spriteBatch.End();
        }

        /// <summary>
        /// DrawModel is a helper function that takes a model, world matrix, and
        /// bone transforms. It does just what its name implies, and draws the model.
        /// </summary>
        /// <param name="model">the model to draw</param>
        /// <param name="worldTransform">where to draw the model</param>
        /// <param name="absoluteBoneTransforms">the model's bone transforms. this can
        /// be calculated using the function Model.CopyAbsoluteBoneTransformsTo</param>
        private void DrawModel(Model model, Matrix worldTransform, 
                               Matrix[] absoluteBoneTransforms, bool drawBoundingSphere)
        {
            // nothing tricky in here; this is the same model drawing code that we see
            // everywhere. we'll loop over all of the meshes in the model, set up their
            // effects, and then draw them.
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();                    

                    effect.View = viewMatrix;
                    effect.Projection = projectionMatrix;
                    effect.World = absoluteBoneTransforms[mesh.ParentBone.Index] * worldTransform;
                }
                mesh.Draw();

                if (drawBoundingSphere)
                {

                     // the mesh's BoundingSphere is stored relative to the mesh itself.
                    // (Mesh space). We want to get this BoundingSphere in terms of world
                    // coordinates. To do this, we calculate a matrix that will transform
                    // from coordinates from mesh space into world space....
                    Matrix world = absoluteBoneTransforms[mesh.ParentBone.Index] * worldTransform;

                    // ... and then transform the BoundingSphere using that matrix.
                    BoundingSphere sphere = TransformBoundingSphere(mesh.BoundingSphere, world);

                    // now draw the sphere with our renderer
                    BoundingSphereRenderer.Draw(sphere, viewMatrix, projectionMatrix);


                }
            }
        }

        /// <summary>
        /// This helper function checks to see if a ray will intersect with a model.
        /// The model's bounding spheres are used, and the model is transformed using
        /// the matrix specified in the worldTransform argument.
        /// </summary>
        /// <param name="ray">the ray to perform the intersection check with</param>
        /// <param name="model">the model to perform the intersection check with.
        /// the model's bounding spheres will be used.</param>
        /// <param name="worldTransform">a matrix that positions the model
        /// in world space</param>
        /// <param name="absoluteBoneTransforms">this array of matrices contains the
        /// absolute bone transforms for the model. this can be obtained using the
        /// Model.CopyAbsoluteBoneTransformsTo function.</param>
        /// <returns>true if the ray intersects the model.</returns>
        private static bool RayIntersectsModel(Ray ray, Model model, 
            Matrix worldTransform, Matrix[] absoluteBoneTransforms)
        {
            // Each ModelMesh in a Model has a bounding sphere, so to check for an
            // intersection in the Model, we have to check every mesh.
            foreach (ModelMesh mesh in model.Meshes)
            {
                // the mesh's BoundingSphere is stored relative to the mesh itself.
                // (Mesh space). We want to get this BoundingSphere in terms of world
                // coordinates. To do this, we calculate a matrix that will transform
                // from coordinates from mesh space into world space....
                Matrix world = absoluteBoneTransforms[mesh.ParentBone.Index] * worldTransform;

                // ... and then transform the BoundingSphere using that matrix.
                BoundingSphere sphere = TransformBoundingSphere(mesh.BoundingSphere, world);

                // now that the we have a sphere in world coordinates, we can just use
                // the BoundingSphere class's Intersects function. Intersects returns a
                // nullable float (float?). This value is the distance at which the ray
                // intersects the BoundingSphere, or null if there is no intersection.
                // so, if the value is not null, we have a collision.
                if (sphere.Intersects(ray) != null)
                {
                    return true;
                }
            }

            // if we've gotten this far, we've made it through every BoundingSphere, and
            // none of them intersected the ray. This means that there was no collision,
            // and we should return false.
            return false;
        }

        /// <summary>
        /// This helper function takes a BoundingSphere and a transform matrix, and
        /// returns a transformed version of that BoundingSphere.
        /// </summary>
        /// <param name="sphere">the BoundingSphere to transform</param>
        /// <param name="world">how to transform the BoundingSphere.</param>
        /// <returns>the transformed BoundingSphere/</returns>
        private static BoundingSphere TransformBoundingSphere(BoundingSphere sphere, Matrix transform)
        {
            BoundingSphere transformedSphere;

            // the transform can contain different scales on the x, y, and z components.
            // this has the effect of stretching and squishing our bounding sphere along
            // different axes. Obviously, this is no good: a bounding sphere has to be a
            // SPHERE. so, the transformed sphere's radius must be the maximum of the 
            // scaled x, y, and z radii.

            // to calculate how the transform matrix will affect the x, y, and z
            // components of the sphere, we'll create a vector3 with x y and z equal
            // to the sphere's radius...
            Vector3 scale3 = new Vector3(sphere.Radius, sphere.Radius, sphere.Radius);

            // then transform that vector using the transform matrix. we use
            // TransformNormal because we don't want to take translation into account.
            scale3 = Vector3.TransformNormal(scale3, transform);

            // scale3 contains the x, y, and z radii of a squished and stretched sphere.
            // we'll set the finished sphere's radius to the maximum of the x y and z
            // radii, creating a sphere that is large enough to contain the original 
            // squished sphere.
            transformedSphere.Radius = Math.Max(scale3.X, Math.Max(scale3.Y, scale3.Z));

            // transforming the center of the sphere is much easier. we can just use 
            // Vector3.Transform to transform the center vector. notice that we're using
            // Transform instead of TransformNormal because in this case we DO want to 
            // take translation into account.
            transformedSphere.Center = Vector3.Transform(sphere.Center, transform);

            return transformedSphere;
        }

        #endregion
    }
}
