#region File Description
//-----------------------------------------------------------------------------
// Material.cs
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
#endregion

namespace MultipassLightingSample
{
    public class Material
    {
        //graphics and Game references
        Effect effectInstance;
        EffectParameter lightsParameterValue;
        ContentManager content;
        GraphicsDevice device;
        private Texture diffuseTexture = null;
        private Texture specularTexture = null;
        private ModelMesh currentMesh = null;
        private ModelMeshPart currentMeshPart = null;


        //shadow parameters
        private float textureURepsValue = 2f;
        private float textureVRepsValue = 2f;
        private float specularPowerValue = 4f;
        private float specularIntensityValue = 200f;
        private string diffuseTextureNameValue = null;
        private string specularTextureNameValue = null;
        private Color colorValue = Color.White;

        #region Initialization
        public Material(ContentManager contentManager, GraphicsDevice graphicsDevice,
            Effect baseEffect)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice");
            }
            device = graphicsDevice;

            if (contentManager == null)
            {
                throw new ArgumentNullException("contentManager");
            }
            content = contentManager;

            if (baseEffect == null)
            {
                throw new ArgumentNullException("baseEffect");
            }

            //clone the material effect instances
            //see the MaterialsAndLights sample for more details
            effectInstance = baseEffect.Clone(device);
            effectInstance.CurrentTechnique =
                effectInstance.Techniques[0];
            device = graphicsDevice;

            // Set the defaults for the effect                                          
            effectInstance.Parameters["specularPower"].SetValue(specularPowerValue);
            effectInstance.Parameters["specularIntensity"].SetValue(
                specularIntensityValue);
            effectInstance.Parameters["materialColor"].SetValue(colorValue.ToVector4());
            effectInstance.Parameters["textureUReps"].SetValue(textureURepsValue);
            effectInstance.Parameters["textureVReps"].SetValue(textureVRepsValue);
            lightsParameterValue = effectInstance.Parameters["lights"];
        }

        public void SetTexturedMaterial(Color color, float specularPower,
            float specularIntensity, string diffuseTextureName,
            string specularTextureName, float textureUReps, float textureVReps)
        {
            Color = color;
            SpecularIntensity = specularIntensity;
            SpecularPower = specularPower;
            DiffuseTexture = diffuseTextureName;
            SpecularTexture = specularTextureName;
            TextureVReps = textureVReps;
            TextureUReps = textureUReps;

        }

        public void SetBasicProperties(Color color, float specularPower, float
            specularIntensity)
        {
            Color = color;
            SpecularIntensity = specularIntensity;
            SpecularPower = specularPower;
        }
        #endregion

        #region Material Properties
        public string SpecularTexture
        {
            set
            {
                specularTextureNameValue = value;
                if (specularTextureNameValue == null)
                {
                    specularTexture = null;
                    effectInstance.Parameters["specularTexture"].SetValue(
                        (Texture)null);
                    effectInstance.Parameters["specularTexEnabled"].SetValue(false);
                }
                else
                {
                    specularTexture = content.Load<Texture>("Textures\\" +
                        specularTextureNameValue);
                    effectInstance.Parameters["specularTexture"].SetValue(
                        specularTexture);
                    effectInstance.Parameters["specularTexEnabled"].SetValue(true);
                }
            }
            get
            {
                return specularTextureNameValue;
            }
        }

        public string DiffuseTexture
        {
            set
            {
                diffuseTextureNameValue = value;
                if (diffuseTextureNameValue == null)
                {
                    diffuseTexture = null;
                    effectInstance.Parameters["diffuseTexture"].SetValue((Texture)null);
                    effectInstance.Parameters["diffuseTexEnabled"].SetValue(false);
                }
                else
                {
                    diffuseTexture = content.Load<Texture>("Textures\\" +
                        diffuseTextureNameValue);
                    effectInstance.Parameters["diffuseTexture"].SetValue(
                        diffuseTexture);
                    effectInstance.Parameters["diffuseTexEnabled"].SetValue(true);
                }
            }
            get
            {
                return diffuseTextureNameValue;
            }
        }


        public Color Color
        {

            set
            {
                colorValue = value;
                effectInstance.Parameters["materialColor"].SetValue(
                    colorValue.ToVector4());
            }
            get
            {
                return colorValue;
            }
        }

        public float SpecularIntensity
        {

            set
            {
                specularIntensityValue = value;
                effectInstance.Parameters["specularIntensity"].SetValue(
                    specularIntensityValue);
            }
            get
            {
                return specularIntensityValue;
            }
        }

        public float SpecularPower
        {

            set
            {
                specularPowerValue = value;
                effectInstance.Parameters["specularPower"].SetValue(specularPowerValue);
            }
            get
            {
                return specularPowerValue;
            }
        }

        public float TextureUReps
        {

            set
            {
                textureURepsValue = value;
                effectInstance.Parameters["textureUReps"].SetValue(textureURepsValue);
            }
            get
            {
                return textureURepsValue;
            }
        }

        public float TextureVReps
        {

            set
            {
                textureVRepsValue = value;
                effectInstance.Parameters["textureVReps"].SetValue(textureVRepsValue);
            }
            get
            {
                return textureVRepsValue;
            }
        }

        public EffectParameter LightsParameter
        {
            get
            {
                return lightsParameterValue;
            }
        }

        #endregion

        #region Draw Function

        /// <summary>
        /// This function sets up the material shader for a batch of draws
        /// performed over multiple lights.  It also renders an ambient
        /// light pass on the geometry.
        /// </summary>
        /// <param name="model">The model that will be drawn in this batch.</param>
        /// <param name="world">
        /// The world matrix for this particular peice of geometry.
        /// </param>
        public void BeginBatch(Model model, ref Matrix world)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            // our sample meshes only contain a single part, so we don't need to bother
            // looping over the ModelMesh and ModelMeshPart collections. If the meshes
            // were more complex, we would repeat all the following code for each part
            currentMesh = model.Meshes[0];
            currentMeshPart = currentMesh.MeshParts[0];

            // set the vertex source to the mesh's vertex buffer
            device.Vertices[0].SetSource(
                currentMesh.VertexBuffer, currentMeshPart.StreamOffset,
                currentMeshPart.VertexStride);

            // set the vertex declaration
            device.VertexDeclaration = currentMeshPart.VertexDeclaration;

            // set the current index buffer to the sample mesh's index buffer
            device.Indices = currentMesh.IndexBuffer;

            //set the world parameter of this instance of the model
            effectInstance.Parameters["world"].SetValue(world);


            effectInstance.Begin(SaveStateMode.None);



            //////////////////////////////////////////////////////////////
            // Example 2.1: State Batching                              //
            //                                                          //
            // In this sample, the BeginBatch() function is responsible //
            // for the one-time drawing of an ambient pass.  This is a  //
            // simplification for the sake of sample code.  The ambient //
            // pass has the dual role of adding an ambient light effect //
            // to the backbuffer, as well as pre-populating the depth   //
            // buffer for the subsequent additive alpha draws.  Also,   //
            // notice that depth is written on this opaque pass, but    //
            // disabled for subsequent passes, as it is unnecessary     //
            // as the geometry does not change.                         //
            //////////////////////////////////////////////////////////////
            device.RenderState.DepthBufferWriteEnable = true;
            device.RenderState.AlphaBlendEnable = false;
            effectInstance.CurrentTechnique.Passes["Ambient"].Begin();
            device.DrawIndexedPrimitives(
                PrimitiveType.TriangleList, currentMeshPart.BaseVertex, 0,
                currentMeshPart.NumVertices, currentMeshPart.StartIndex,
                currentMeshPart.PrimitiveCount);
            effectInstance.CurrentTechnique.Passes["Ambient"].End();

            //Set up renderstates for point-light contributions
            device.RenderState.AlphaBlendEnable = true;
            device.RenderState.DepthBufferWriteEnable = false;
            effectInstance.CurrentTechnique.Passes["PointLight"].Begin();

        }

        /// <summary>
        /// The draw function for the material has been moved off to
        /// the material.  Sorting draws on the material is generally a
        /// good way to reduce state switching and thusly CPU performance
        /// overhead.
        /// </summary>
        public void DrawModel()
        {
            if (currentMesh == null)
            {
                throw new InvalidOperationException(
                    "DrawModel may only be called between begin/end pairs.");
            }

            //////////////////////////////////////////////////////////////
            // Example 2.2: CommitChanges()                             //
            //                                                          //
            // In this draw function, we're assuming that the light     //
            // states have been changed.  Therefore, CommitChanges()    //
            // is called implicitly to get the latest updates to the    //
            // "dirty" effect paramters before submitting the draw.     //
            //////////////////////////////////////////////////////////////
            effectInstance.CommitChanges();

            // sampleMesh contains all of the information required to draw
            // the current mesh
            device.DrawIndexedPrimitives(
                PrimitiveType.TriangleList, currentMeshPart.BaseVertex, 0,
                currentMeshPart.NumVertices, currentMeshPart.StartIndex, 
                currentMeshPart.PrimitiveCount);
        }

        public void EndBatch()
        {
            // EffectPass.End must be called when the effect is no longer needed
            effectInstance.CurrentTechnique.Passes["PointLight"].End();

            // likewise, Effect.End will end the current technique
            effectInstance.End();

            currentMeshPart = null;
            currentMesh = null;
        }
        #endregion
    }
}
