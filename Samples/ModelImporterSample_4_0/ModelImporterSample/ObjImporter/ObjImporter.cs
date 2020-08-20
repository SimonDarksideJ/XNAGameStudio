#region File Description
//-----------------------------------------------------------------------------
// ObjImporter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.Globalization;
#endregion

namespace ObjImporterSample
{
    [ContentImporter(".obj", CacheImportedData=true, DefaultProcessor="ModelProcessor")]
    public class ObjImporter : ContentImporter<NodeContent>
    {
        #region Variables


        // Provides the logger and allows for tracking of file dependencies
        ContentImporterContext importerContext;


        // The root NodeContent of our model
        private NodeContent rootNode;

        // All vertex data in the file
        private List<Vector3> positions;
        private List<Vector2> texCoords;
        private List<Vector3> normals;


        // The current mesh being constructed
        private MeshBuilder meshBuilder;

        // Mapping from mesh positions to the complete list of
        // positions for the current mesh
        private int[] positionMap;

        // Indices of vertex channels for the current mesh
        private int textureCoordinateDataIndex;
        private int normalDataIndex;


        // Named materials from all imported MTL files
        private Dictionary<String, MaterialContent> materials;

        // Identity of current MTL file for reporting errors agaisnt
        private ContentIdentity mtlFileIdentity;

        // Current material being constructed
        private BasicMaterialContent materialContent;


        #endregion

        #region Entry point


        /// <summary>
        /// The importer's entry point.
        /// Called by the framework when importing a game asset.
        /// </summary>
        /// <param name="filename">Name of a game asset file.</param>
        /// <param name="context">
        /// Contains information for importing a game asset, such as a logger interface.
        /// </param>
        /// <returns>Resulting game asset.</returns>
        public override NodeContent Import(string filename,
            ContentImporterContext context)
        {
            // Uncomment the following line to debug:
            //System.Diagnostics.Debugger.Launch();

            // Store the context for use in other methods
            importerContext = context;

            // Reset all importer state
            // See field declarations for more information
            rootNode = new NodeContent();            
            positions = new List<Vector3>();
            texCoords = new List<Vector2>();
            normals = new List<Vector3>();
            meshBuilder = null;
            // StartMesh sets positionMap, textureCoordinateDataIndex, normalDataIndex
            materials = new Dictionary<string, MaterialContent>();
            // ImportMaterials resets materialContent

            // Model identity is tied to the file it is loaded from
            rootNode.Identity = new ContentIdentity(filename);

            try
            {
                // Loop over each tokenized line of the OBJ file
                foreach (String[] lineTokens in
                    GetLineTokens(filename, rootNode.Identity))
                {
                    ParseObjLine(lineTokens);
                }

                // If the file did not provide a model name (through an 'o' line),
                // then use the file name as a default
                if (rootNode.Name == null)
                    rootNode.Name = Path.GetFileNameWithoutExtension(filename);

                // Finish the last mesh
                FinishMesh();

                // Done with entire model!
                return rootNode;
            }
            catch (InvalidContentException)
            {
                // InvalidContentExceptions do not need further processing
                throw;
            }
            catch (Exception e)
            {
                // Wrap exception with content identity (includes line number)
                throw new InvalidContentException(
                    "Unable to parse obj file. Exception:\n" + e.Message,
                    rootNode.Identity, e);
            }
        }

        #endregion

        #region Mesh parsing


        /// <summary>
        /// Parses and executes an individual line of an OBJ file.
        /// </summary>
        /// <param name="lineTokens">Line to parse as tokens</param>
        private void ParseObjLine(string[] lineTokens)
        {
            // Switch by line type
            switch (lineTokens[0].ToLower())
            {
                // Object
                case "o":
                    // The next token is the name of the model
                    rootNode.Name = lineTokens[1];
                    break;

                // Positions
                case "v":
                    positions.Add(ParseVector3(lineTokens));
                    break;

                // Texture coordinates
                case "vt":
                {
                    // u is required, but v and w are optional
                    // Require a Vector2 and ignore the w for the sake of this sample
                    Vector2 vt = ParseVector2(lineTokens);

                    // Flip the v coordinate
                    vt.Y = 1 - vt.Y;

                    texCoords.Add(vt);
                    break;
                }

                // Normals
                case "vn":
                    normals.Add(ParseVector3(lineTokens));
                    break;

                // Groups (model meshes)
                case "g":
                    // End the current mesh
                    if (meshBuilder != null)
                        FinishMesh();

                    // Begin a new mesh
                    // The next token is an optional name
                    if (lineTokens.Length > 1)
                        StartMesh(lineTokens[1]);
                    else
                        StartMesh(null);
                    break;

                // Smoothing group
                case "s":
                    // Ignore; just use the normals as specified with verticies
                    break;

                // Faces (only triangles are supported)
                case "f":
                    // Warn about and ignore polygons which are not triangles
                    if (lineTokens.Length > 4)
                    {
                        importerContext.Logger.LogWarning(null, rootNode.Identity,
                            "N-sided polygons are not supported; Ignoring face");
                        break;
                    }

                    // If the builder is null, this face is outside of a group
                    // Start a new, unnamed group
                    if (meshBuilder == null)
                        StartMesh(null);

                    // For each triangle vertex
                    for (int vertexIndex = 1; vertexIndex <= 3; vertexIndex++)
                    {
                        // Each vertex is a set of three indices:
                        // position, texture coordinate, and normal
                        // The indices are 1-based, separated by slashes
                        // and only position is required.
                        string[] indices = lineTokens[vertexIndex].Split('/');

                        // Required: Position
                        int positionIndex = int.Parse(indices[0], 
                            CultureInfo.InvariantCulture) - 1;

                        if (indices.Length > 1)
                        {
                            // Optional: Texture coordinate
                            int texCoordIndex;
                            Vector2 texCoord;
                            if (int.TryParse(indices[1], out texCoordIndex))
                                texCoord = texCoords[texCoordIndex - 1];
                            else
                                texCoord = Vector2.Zero;

                            // Set channel data for texture coordinate for the following
                            // vertex. This must be done before calling AddTriangleVertex
                            meshBuilder.SetVertexChannelData(textureCoordinateDataIndex,
                                texCoord);
                        }

                        if (indices.Length > 2)
                        {
                            // Optional: Normal
                            int normalIndex;
                            Vector3 normal;
                            if (int.TryParse(indices[2], out normalIndex))
                                normal = normals[normalIndex - 1];
                            else
                                normal = Vector3.Zero;

                            // Set channel data for normal for the following vertex.
                            // This must be done before calling AddTriangleVertex
                            meshBuilder.SetVertexChannelData(normalDataIndex,
                                normal);

                        }

                        // Add the vertex with the vertex data that was just set
                        meshBuilder.AddTriangleVertex(positionMap[positionIndex]);
                    }
                    break;

                // Import a material library file
                case "mtllib":
                    // Remaining tokens are relative paths to MTL files
                    for (int i = 1; i < lineTokens.Length; i++)
                    {
                        string mtlFileName = lineTokens[i];

                        // A full path is needed,
                        if (!Path.IsPathRooted(mtlFileName))
                        {
                            // resolve relative paths
                            string directory =
                                Path.GetDirectoryName(rootNode.Identity.SourceFilename);
                            mtlFileName = Path.GetFullPath(
                                Path.Combine(directory, mtlFileName));
                        }

                        // By calling AddDependency, we will cause this model
                        // to be rebuilt if its associated MTL files change
                        importerContext.AddDependency(mtlFileName);

                        // Import and record the new materials
                        ImportMaterials(mtlFileName);
                    }
                    break;

                // Apply a material 
                case "usemtl":
                {
                    // If the builder is null, OBJ most likely lacks groups
                    // Start a new, unnamed group
                    if (meshBuilder == null)
                        StartMesh(null);

                    // Next token is material name
                    string materialName = lineTokens[1];

                    // Apply the material to the upcoming faces
                    MaterialContent material;
                    if (materials.TryGetValue(materialName, out material))
                        meshBuilder.SetMaterial(material);
                    else
                    {
                        throw new InvalidContentException(String.Format(
                            "Material '{0}' not defined.", materialName),
                            rootNode.Identity);
                    }

                    break;
                }

                // Unsupported or invalid line types
                default:
                    throw new InvalidContentException(
                        "Unsupported or invalid line type '" + lineTokens[0] + "'",
                        rootNode.Identity);
            }
        }


        /// <summary>
        /// Starts a new mesh and fills it with mesh mapped positions.
        /// </summary>
        /// <param name="name">Name of mesh.</param>
        private void StartMesh(string name)
        {
            meshBuilder = MeshBuilder.StartMesh(name);

            // Obj files need their winding orders swapped
            meshBuilder.SwapWindingOrder = true;

            // Add additional vertex channels for texture coordinates and normals
            textureCoordinateDataIndex = meshBuilder.CreateVertexChannel<Vector2>(
                VertexChannelNames.TextureCoordinate(0));
            normalDataIndex =
                meshBuilder.CreateVertexChannel<Vector3>(VertexChannelNames.Normal());

            // Add each position to this mesh with CreatePosition
            positionMap = new int[positions.Count];
            for (int i = 0; i < positions.Count; i++)
            {
                // positionsMap redirects from the original positions in the order
                // they were read from file to indices returned from CreatePosition
                positionMap[i] = meshBuilder.CreatePosition(positions[i]);
            }
        }


        /// <summary>
        /// Finishes building a mesh and adds the resulting MeshContent or
        /// NodeContent to the root model's NodeContent.
        /// </summary>
        private void FinishMesh()
        {
            MeshContent meshContent = meshBuilder.FinishMesh();

            // Groups without any geometry are just for transform
            if (meshContent.Geometry.Count > 0)
            {
                // Add the mesh to the model
                rootNode.Children.Add(meshContent);
            }
            else
            {
                // Convert to a general NodeContent
                NodeContent nodeContent = new NodeContent();
                nodeContent.Name = meshContent.Name;

                // Add the transform-only node to the model
                rootNode.Children.Add(nodeContent);
            }

            meshBuilder = null;
        }


        #endregion

        #region Material parsing


        /// <summary>
        /// Parses an MTL file and adds all its materials to the materials collection
        /// </summary>
        /// <param name="filename">Full path of MTL file.</param>
        private void ImportMaterials(string filename)
        {
            // Material library identity is tied to the file it is loaded from
            mtlFileIdentity = new ContentIdentity(filename);
            
            // Reset the current material
            materialContent = null;

            try
            {
                // Loop over each tokenized line of the MTL file
                foreach (String[] lineTokens in
                    GetLineTokens(filename, mtlFileIdentity))
                {
                    ParseMtlLine(lineTokens);
                }
            }
            catch (InvalidContentException)
            {
                // InvalidContentExceptions do not need further processing
                throw;
            }
            catch (Exception e)
            {
                // Wrap exception with content identity (includes line number)
                throw new InvalidContentException(
                    "Unable to parse mtl file. Exception:\n" + e.Message,
                    mtlFileIdentity, e);
            }

            // Finish the last material
            if (materialContent != null)
                materials.Add(materialContent.Name, materialContent);
        }


        /// <summary>
        /// Parses and executes an individual line of a MTL file.
        /// </summary>
        /// <param name="lineTokens">Line to parse as tokens</param>
        void ParseMtlLine(string[] lineTokens)
        {
            // Switch on line type
            switch (lineTokens[0].ToLower())
            {
                // New material
                case "newmtl":
                    // Finish the current material
                    if (materialContent != null)
                        materials.Add(materialContent.Name, materialContent);

                    // Start a new material
                    materialContent = new BasicMaterialContent();
                    materialContent.Identity =
                        new ContentIdentity(mtlFileIdentity.SourceFilename);

                    // Next token is new material's name
                    materialContent.Name = lineTokens[1];
                    break;

                // Diffuse color
                case "kd":
                    materialContent.DiffuseColor = ParseVector3(lineTokens);
                    break;

                // Diffuse texture
                case "map_kd":
                    // Reference a texture relative to this MTL file
                    materialContent.Texture = new ExternalReference<TextureContent>(
                        lineTokens[1], mtlFileIdentity);
                    break;

                // Ambient color
                case "ka":
                    // Ignore ambient color because it should be set by scene lights
                    break;

                // Specular color
                case "ks":
                    materialContent.SpecularColor = ParseVector3(lineTokens);
                    break;

                // Specular power
                case "ns":
                    materialContent.SpecularPower = float.Parse(lineTokens[1],
                            CultureInfo.InvariantCulture);
                    break;

                // Emissive color
                case "ke":
                    materialContent.EmissiveColor = ParseVector3(lineTokens);
                    break;

                // Alpha
                case "d":
                    materialContent.Alpha = float.Parse(lineTokens[1],
                            CultureInfo.InvariantCulture);
                    break;

                // Illumination mode (0=constant, 1=diffuse, 2=diffuse+specular)
                case "illum":
                    // Store as opaque data. This alllows the rendering engine to
                    // interpret this data if it likes. For example, it can set
                    // constant constant illumination mode by dissabling lighting
                    // on the BasicEffect.
                    materialContent.OpaqueData.Add("Illumination mode",
                        int.Parse(lineTokens[1], CultureInfo.InvariantCulture));
                    break;

                // Unsupported or invalid line types
                default:
                    throw new InvalidContentException(
                        "Unsupported or invalid line type '" + lineTokens[0] + "'",
                        mtlFileIdentity);
            }
        }


        #endregion

        #region Shared parsing helpers


        /// <summary>
        /// Yields an array of tokens for each line in an OBJ or MTL file.
        /// </summary>
        /// <remarks>
        /// OBJ and MTL files are text based formats of identical structure.
        /// Each line of a OBJ or MTL file is either an instruction or a comment.
        /// Comments begin with # and are effectively ignored.
        /// Instructions are a space dilimited list of tokens. The first token is the
        /// instruction type code. The tokens which follow, are the arguments to that
        /// instruction. The number and format of arguments vary by instruction type.
        /// </remarks>
        /// <param name="filename">Full path of file to read.</param>
        /// <param name="identity">Identity of the file being read. This is modified to
        /// include the current line number in case an exception is thrown.</param>
        /// <returns>Element 0 is the line type identifier. The remaining elements are
        /// arguments to the identifier's operation.</returns>
        private static IEnumerable<string[]> GetLineTokens(string filename,
            ContentIdentity identity)
        {
            // Open the file
            using (StreamReader reader = new StreamReader(filename))
            {
                int lineNumber = 1;

                // For each line of the file
                while (!reader.EndOfStream)
                {
                    // Set the line number to report in case an exception is thrown
                    identity.FragmentIdentifier = lineNumber.ToString();

                    // Tokenize line by splitting on 1 more more whitespace character
                    string[] lineTokens = Regex.Split(reader.ReadLine().Trim(), @"\s+");

                    // Skip blank lines and comments
                    if (lineTokens.Length > 0 &&
                        lineTokens[0] != String.Empty &&
                        !lineTokens[0].StartsWith("#"))
                    {
                        // Pass off the tokens of this line to be processed
                        yield return lineTokens;
                    }

                    // Done with this line!
                    lineNumber++;
                }


                // Clear line number from identity
                identity.FragmentIdentifier = null;
            }
        }


        /// <summary>
        /// Parses a Vector2 from tokens of an OBJ file line.
        /// </summary>
        /// <param name="lineTokens">X and Y coordinates in lineTokens[1 through 2].
        /// </param>
        private static Vector2 ParseVector2(string[] lineTokens)
        {
            return new Vector2(
                float.Parse(lineTokens[1], CultureInfo.InvariantCulture),
                float.Parse(lineTokens[2], CultureInfo.InvariantCulture));
        }


        /// <summary>
        /// Parses a Vector3 from tokens of an OBJ file line.
        /// </summary>
        /// <param name="lineTokens">X,Y and Z coordinates in lineTokens[1 through 3].
        /// </param>
        private static Vector3 ParseVector3(string[] lineTokens)
        {
            return new Vector3(
                float.Parse(lineTokens[1], CultureInfo.InvariantCulture),
                float.Parse(lineTokens[2], CultureInfo.InvariantCulture), 
                float.Parse(lineTokens[3], CultureInfo.InvariantCulture));
        }


        #endregion
    }
}
