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

namespace MaterialsAndLightsSample
{
    public class Material
    {
        //graphics and Game references
        Effect effectInstance;
        ContentManager content;
        GraphicsDevice device;
        private Texture diffuseTexture = null;
        private Texture specularTexture = null;

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

            //////////////////////////////////////////////////////////////
            // Example 2.1                                              //
            //                                                          //
            // There are many ways to store and organize the constants  //
            // used for a material.  For this example, an entiire       //
            // Effect instance is cloned from the basic material shader //
            // and the material parameters are bound to that instance.  //
            // The result is a vastly simplified way of rendering a     //
            // material by simply setting the appropriate shader and    //
            // starting to draw.  This is also a very efficient         //
            // technique on the XBox 360, but typically less so on a    //
            // PC.                                                      //
            //////////////////////////////////////////////////////////////
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



        #endregion

        #region Draw Function
        /// <summary>
        /// The draw function for the material has been moved off to
        /// the material.  Sorting draws on the material is generally a
        /// good way to reduce state switching and thusly CPU performance
        /// overhead.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="world"></param>
        public void DrawModelWithMaterial(Model model, ref Matrix world)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            // our sample meshes only contain a single part, so we don't need to bother
            // looping over the ModelMesh and ModelMeshPart collections. If the meshes
            // were more complex, we would repeat all the following code for each part
            ModelMesh mesh = model.Meshes[0];
            ModelMeshPart meshPart = mesh.MeshParts[0];

            // set the vertex source to the mesh's vertex buffer
            device.Vertices[0].SetSource(
                mesh.VertexBuffer, meshPart.StreamOffset, meshPart.VertexStride);

            // set the vertex declaration
            device.VertexDeclaration = meshPart.VertexDeclaration;

            // set the current index buffer to the sample mesh's index buffer
            device.Indices = mesh.IndexBuffer;

            //set the world parameter of this instance of the model
            effectInstance.Parameters["world"].SetValue(world);

            //////////////////////////////////////////////////////////////
            // Example 2.2                                              //
            //                                                          //
            // On Effect Instance.Begin(), the effect states are        //
            // applied to the device.  This can be an expensive call    //
            // if there are lots of paramters in a given scene.         //
            // Aggressive optimization can yeild decent gains here,     //
            // particularly on PC hardware, but for clarity, this       //
            // sample is using Effect Pools which will re-set shared    //
            // paramters each time, even if they do not change between  //
            // effect instances.                                        //
            //////////////////////////////////////////////////////////////
            effectInstance.Begin(SaveStateMode.None);

            // EffectPass.Begin will update the device to
            // begin using the state information defined in the current pass
            effectInstance.CurrentTechnique.Passes[0].Begin();

            // sampleMesh contains all of the information required to draw
            // the current mesh
            device.DrawIndexedPrimitives(
                PrimitiveType.TriangleList, meshPart.BaseVertex, 0,
                meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);

            // EffectPass.End must be called when the effect is no longer needed
            effectInstance.CurrentTechnique.Passes[0].End();

            // likewise, Effect.End will end the current technique
            effectInstance.End();

        }
        #endregion
    }
}
