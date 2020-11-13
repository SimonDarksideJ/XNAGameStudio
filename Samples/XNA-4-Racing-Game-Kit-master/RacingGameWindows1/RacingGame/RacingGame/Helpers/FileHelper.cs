#region File Description
//-----------------------------------------------------------------------------
// FileHelper.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework.GamerServices;
using RacingGame.Graphics;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace RacingGame.Helpers
{
    /// <summary>
    /// File helper class to get text lines, number of text lines, etc.
    /// Update: Now also supports the XNA Storage classes :)
    /// </summary>
    public static class FileHelper
    {
        #region LoadGameContentFile
        /// <summary>
        /// Load game content file, returns null if file was not found.
        /// </summary>
        /// <param name="relativeFilename">Relative filename</param>
        /// <returns>File stream</returns>
        public static Stream LoadGameContentFile(string relativeFilename)
        {
            return TitleContainer.OpenStream(relativeFilename);
        }
        #endregion

        #region StorageDevice

        public static ManualResetEvent StorageContainerMRE = new ManualResetEvent(true);


        /// <summary>
        /// XNA user device, asks for the saving location on the Xbox360,
        /// theirfore remember this device for the time we run the game.
        /// </summary>
        static StorageDevice xnaUserDevice = null;

        /// <summary>
        /// Xna user device
        /// </summary>
        /// <returns>Storage device</returns>
        public static StorageDevice XnaUserDevice
        {
            get
            {
                if ((xnaUserDevice != null) && !xnaUserDevice.IsConnected)
                {
                    xnaUserDevice = null;
                }
                // Create if not created yet.
                if (xnaUserDevice == null)
                {
                    if (Guide.IsVisible)
                    {
                        return null;
                    }
                    IAsyncResult async = StorageDevice.BeginShowSelector(PlayerIndex.One, null, null);

                    async.AsyncWaitHandle.WaitOne();

                    xnaUserDevice = StorageDevice.EndShowSelector(async);
#if XBOX360
                    if (!Guide.IsVisible)
                    {
                        BaseGame.GamerServicesComponent.Update(new GameTime());
                    }
                    if (Guide.IsVisible)
                    {
                        Thread.Sleep(10);
                        BaseGame.GamerServicesComponent.Update(new GameTime());
                        BaseGame.graphicsManager.GraphicsDevice.Clear(Color.Black);
                        BaseGame.graphicsManager.GraphicsDevice.Present();
                    }
#endif
                }
                return xnaUserDevice;
            }
        }

        #endregion

        #region Get text lines
        /// <summary>
        /// Returns the number of text lines we got in a file.
        /// </summary>
        static public string[] GetLines(string filename)
        {
            try
            {
                StreamReader reader = new StreamReader(
                    new FileStream(filename, FileMode.Open, FileAccess.Read),
                    System.Text.Encoding.UTF8);
                // Generic version
                List<string> lines = new List<string>();
                do
                {
                    lines.Add(reader.ReadLine());
                } while (reader.Peek() > -1);
                reader.Close();
                return lines.ToArray();
            }
            catch (FileNotFoundException)
            {
                // Failed to read, just return null!
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }
            catch (IOException)
            {
                return null;
            }
        }
        #endregion

        #region Write Helpers
        /// <summary>
        /// Write vector3 to stream
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="vec">Vector3</param>
        public static void WriteVector3(BinaryWriter writer, Vector3 vec)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            writer.Write(vec.X);
            writer.Write(vec.Y);
            writer.Write(vec.Z);
        }

        /// <summary>
        /// Write vector4 to stream
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="vec">Vector4</param>
        public static void WriteVector4(BinaryWriter writer, Vector4 vec)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            writer.Write(vec.X);
            writer.Write(vec.Y);
            writer.Write(vec.Z);
            writer.Write(vec.W);
        }

        /// <summary>
        /// Write matrix to stream
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="matrix">Matrix</param>
        public static void WriteMatrix(BinaryWriter writer, Matrix matrix)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            writer.Write(matrix.M11);
            writer.Write(matrix.M12);
            writer.Write(matrix.M13);
            writer.Write(matrix.M14);
            writer.Write(matrix.M21);
            writer.Write(matrix.M22);
            writer.Write(matrix.M23);
            writer.Write(matrix.M24);
            writer.Write(matrix.M31);
            writer.Write(matrix.M32);
            writer.Write(matrix.M33);
            writer.Write(matrix.M34);
            writer.Write(matrix.M41);
            writer.Write(matrix.M42);
            writer.Write(matrix.M43);
            writer.Write(matrix.M44);
        }
        #endregion

        #region Read Helpers
        /// <summary>
        /// Read vector3 from stream
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <returns>Vector3</returns>
        public static Vector3 ReadVector3(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            return new Vector3(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle());
        }

        /// <summary>
        /// Read vector4 from stream
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <returns>Vector4</returns>
        public static Vector4 ReadVector4(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            return new Vector4(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle());
        }

        /// <summary>
        /// Read matrix from stream
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <returns>Matrix</returns>
        public static Matrix ReadMatrix(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            return new Matrix(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle());
        }
        #endregion
    }
}
