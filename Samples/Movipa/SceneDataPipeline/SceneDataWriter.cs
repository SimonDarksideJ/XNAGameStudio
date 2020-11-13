#region File Description
//-----------------------------------------------------------------------------
// SceneDataWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using SceneDataLibrary;

using TInput = System.String;
using TOutput = System.String;
#endregion


namespace SceneDataLibraryPipeline
{
    /// <summary>
    /// This class writes scene data.
    /// </summary>
    [ContentTypeWriter]
    public class SceneDataWriter : ContentTypeWriter<SceneData>
    {
        /// <summary>
        /// Writes all the information in the scene.
        /// </summary>
        protected override void Write(ContentWriter output, SceneData value)
        {
            Dictionary<String, PatternGroupData> listPatBody = 
                value.PatternGroupDictionary;

            output.Write(listPatBody.Count);//The number of pattern groups

            Dictionary<String, PatternGroupData>.Enumerator enumPatBody = 
                listPatBody.GetEnumerator();

            //Writes a pattern.
            while (enumPatBody.MoveNext())
            {
                output.Write(enumPatBody.Current.Key);
                WritePatternGroupData(output, enumPatBody.Current.Value);
            }

            Dictionary<String, SequenceBankData> listSeqData = 
                value.SequenceBankDictionary;

            output.Write(listSeqData.Count);//The number of sequence banks

            Dictionary<String, SequenceBankData>.Enumerator enumSeqData = 
                listSeqData.GetEnumerator();

            //Writes a sequence.
            while (enumSeqData.MoveNext())
            {
                output.Write(enumSeqData.Current.Key);
                WriteSequenceBankData(output, enumSeqData.Current.Value);
            }
        }

        /// <summary>
        /// Performs override from the parent class.
        /// </summary>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(SceneDataReader).AssemblyQualifiedName;
        }


        #region Helper Methods


        /// <summary>
        /// Writes the pattern object.
        /// </summary>
        private static void WritePatternObjectData(ContentWriter output, 
                PatternObjectData partsPat)
        {
            output.Write(partsPat.TextureName);//Texture name
            output.Write(partsPat.Rect.X);//Cutting position X
            output.Write(partsPat.Rect.Y);//Cutting position Y
            output.Write(partsPat.Rect.Width);//Width
            output.Write(partsPat.Rect.Height);//Height
            output.Write(partsPat.FlipH);//Horizontal flip
            output.Write(partsPat.FlipV);//Vertical flip
            output.Write(partsPat.Position.X);//Display position X
            output.Write(partsPat.Position.Y);//Display position Y
            output.Write((Byte)(partsPat.Color.R * 0xFF / 0x80));//Color: Red
            output.Write((Byte)(partsPat.Color.G * 0xFF / 0x80));//Color: Green
            output.Write((Byte)(partsPat.Color.B * 0xFF / 0x80));//Color: Blue
            output.Write((Byte)(partsPat.Color.A * 0xFF / 0x80));//Color: Clarity
            output.Write(partsPat.Scale);//Scale
            output.Write(partsPat.Center.X);//Center point X
            output.Write(partsPat.Center.Y);//Center point Y
            output.Write(partsPat.RotateZ);//Rotation value
        }

        /// <summary>
        /// Writes a pattern group.
        /// </summary>
        private static void WritePatternGroupData(ContentWriter output, 
                PatternGroupData bodyPat)
        {
            List<PatternObjectData> listPatParts = bodyPat.PatternObjectList;

            //The number of pattern objects
            output.Write(listPatParts.Count);

            foreach (PatternObjectData partsPat in listPatParts)
                WritePatternObjectData(output, partsPat);
        }

        /// <summary>
        /// Writes a sequence object.
        /// </summary>
        private static void WriteSequenceObjectData(ContentWriter output, 
                SequenceObjectData partsSeq)
        {
            //The number of frames to be displayed
            output.Write(partsSeq.Frame);
            output.Write(partsSeq.PatternGroupName);
        }

        /// <summary>
        /// Writes a sequence group.
        /// </summary>
        private static void WriteSequenceGroupData(ContentWriter output, 
                SequenceGroupData bodySeq)
        {
            output.Write(bodySeq.StartFrame);//Start frame
            output.Write(bodySeq.LoopNumber);//Loop count
            output.Write((byte)bodySeq.InterpolationType);//Interpolation type
            output.Write(bodySeq.SplineParamT);//Interpolation parameter T
            output.Write(bodySeq.SplineParamC);//Interpolation parameter C
            output.Write(bodySeq.SplineParamB);//Interpolation parameter B

            List<SequenceObjectData> listSeqParts = bodySeq.SequenceObjectList;

            output.Write(listSeqParts.Count);//The number of sequence objects

            foreach (SequenceObjectData partsSeq in listSeqParts)
                WriteSequenceObjectData(output, partsSeq);
        }
        
        /// <summary>
        /// Writes a sequence bank.
        /// </summary>
        private static void WriteSequenceBankData(ContentWriter output, 
                SequenceBankData dataSeq)
        {
            List<SequenceGroupData> listSeqBody = dataSeq.SequenceGroupList;

            output.Write(dataSeq.ZPos);//Display priority
            output.Write(listSeqBody.Count);//The number of sequence groups

            foreach (SequenceGroupData bodySeq in listSeqBody)
                WriteSequenceGroupData(output, bodySeq);
        }


        #endregion
    }
}
