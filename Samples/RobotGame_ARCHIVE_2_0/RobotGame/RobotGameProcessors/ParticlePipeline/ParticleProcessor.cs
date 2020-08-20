#region File Description
//-----------------------------------------------------------------------------
// ParticleProcessor.cs
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
using RobotGameData.ParticleSystem;
#endregion

namespace ParticlePipeline
{
    public class ImportContentData
    {
        ParticleSequenceInfo data = null;

        public ParticleSequenceInfo Data
        {
            get { return data; }
        }

        public ImportContentData()
        {
            this.data = new ParticleSequenceInfo();
        }

        public ImportContentData(ParticleSequenceInfo data)
        {
            this.data = data;
        }
    }

    /// <summary>
    /// Custom Content Pipeline importer class for ParticleSequenceInfo data.
    /// </summary>
    [ContentImporter(".Particle", CacheImportedData = true, 
                                            DefaultProcessor = "ParticleProcessor")]
    public class ParticleImporter : ContentImporter<ImportContentData>
    {
        public override ImportContentData Import(string filename, 
                                                    ContentImporterContext context)
        {
            Stream stream = File.OpenRead(Path.Combine("Content", filename));

            XmlTextReader reader = new XmlTextReader(stream);
            XmlSerializer serializer = new XmlSerializer(typeof(ParticleSequenceInfo));

            ParticleSequenceInfo info = 
                                    (ParticleSequenceInfo)serializer.Deserialize(reader);

            return new ImportContentData(info);
        }
    }

    /// <summary>
    /// Custom Content Pipeline processor class for ParticleSequenceInfo data.
    /// </summary>
    [ContentProcessor]
    public class ParticleProcessor : 
        ContentProcessor<ImportContentData, WriteContentData>
    {
        public override WriteContentData Process(ImportContentData input, 
                                                ContentProcessorContext context)
        {
            return new WriteContentData(input.Data);
        }
    }
}
