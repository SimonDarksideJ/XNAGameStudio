#region File Description
//-----------------------------------------------------------------------------
// AnimationProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework;
using RobotGameData.GameObject;
#endregion

namespace AnimationPipeline
{
    public class ImportContentData
    {
        AnimationSequence data = null;

        public AnimationSequence Data
        {
            get { return data; }
        }

        public ImportContentData()
        {
            this.data = new AnimationSequence();
        }

        public ImportContentData(AnimationSequence data)
        {
            this.data = data;
        }
    }

    /// <summary>
    /// Custom Content Pipeline importer class for AnimationSequence data.
    /// </summary>
    [ContentImporter(".Animation", CacheImportedData = true,
                                           DefaultProcessor = "AnimationProcessor")]
    public class AnimationImporter : ContentImporter<ImportContentData>
    {
        public override ImportContentData Import(string filename,
                                                    ContentImporterContext context)
        {
            Stream stream = File.OpenRead(Path.Combine("Content", filename));

            XmlTextReader reader = new XmlTextReader(stream);
            XmlSerializer serializer = new XmlSerializer(typeof(AnimationSequence));
            AnimationSequence data = (AnimationSequence)serializer.Deserialize(reader);

            return new ImportContentData(data);
        }
    }

    /// <summary>
    /// Custom Content Pipeline processor class for AnimationSequence data.
    /// </summary>
    [ContentProcessor]
    public class AnimationProcessor : 
        ContentProcessor<ImportContentData, WriteContentData>
    {
        public override WriteContentData Process(ImportContentData input,
                                                ContentProcessorContext context)
        {
            return new WriteContentData(input.Data);
        }
    }
}
