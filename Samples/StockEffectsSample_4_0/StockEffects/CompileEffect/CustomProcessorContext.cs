#region File Description
//-----------------------------------------------------------------------------
// CustomProcessorContext.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
#endregion

namespace CompileEffect
{
    /// <summary>
    /// Custom context object for running Content Pipeline processors.
    /// </summary>
    class CustomProcessorContext : ContentProcessorContext
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CustomProcessorContext(TargetPlatform targetPlatform, GraphicsProfile targetProfile, ContentBuildLogger logger)
        {
            this.targetPlatform = targetPlatform;
            this.targetProfile = targetProfile;
            this.logger = logger;
        }


        /// <summary>
        /// Gets the build target platform.
        /// </summary>
        public override TargetPlatform TargetPlatform
        {
            get { return targetPlatform; } 
        }

        TargetPlatform targetPlatform;


        /// <summary>
        /// Gets the build target profile.
        /// </summary>
        public override GraphicsProfile TargetProfile
        {
            get { return targetProfile; } 
        }

        GraphicsProfile targetProfile;


        /// <summary>
        /// Gets a logger for reporting content build messages and warnings.
        /// </summary>
        public override ContentBuildLogger Logger
        {
            get { return logger; }
        }

        ContentBuildLogger logger;


        /// <summary>
        /// Gets the current build configuration (Debug, Release, etc).
        /// </summary>
        public override string BuildConfiguration
        {
            get { return string.Empty; } 
        }


        /// <summary>
        /// Gets the intermediate directory for the current content build.
        /// </summary>
        public override string IntermediateDirectory
        {
            get { return string.Empty; } 
        }


        /// <summary>
        /// Gets the final output directory for the current content build.
        /// </summary>
        public override string OutputDirectory
        {
            get { return string.Empty; } 
        }


        /// <summary>
        /// Gets the final output filename for the asset currently being built.
        /// </summary>
        public override string OutputFilename
        {
            get { return string.Empty; } 
        }


        /// <summary>
        /// Dictionary can be used to pass custom parameter data into the processor.
        /// </summary>
        public override OpaqueDataDictionary Parameters
        {
            get { return parameters; } 
        }

        OpaqueDataDictionary parameters = new OpaqueDataDictionary();


        /// <summary>
        /// Records any additional input files that were read by the processor.
        /// </summary>
        public override void AddDependency(string filename)
        { 
        }


        /// <summary>
        /// Records any additional output files that were written by the processor.
        /// </summary>
        public override void AddOutputFile(string filename)
        { 
        }


        /// <summary>
        /// Asks the Content Pipeline to call into a different processor,
        /// converting an object in memory to a different format.
        /// </summary>
        public override TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters)
        {
            throw new NotImplementedException(); 
        }


        /// <summary>
        /// Asks the Content Pipeline to call into a different importer and processor,
        /// reading from a source asset file and returning an object in memory.
        /// </summary>
        public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName)
        {
            throw new NotImplementedException(); 
        }


        /// <summary>
        /// Asks the Content Pipeline to call into a different importer and processor, building
        /// a source asset file into .xnb format, and returning a reference to the output .xnb.
        /// </summary>
        public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName, string assetName)
        {
            throw new NotImplementedException(); 
        }
    }
}
