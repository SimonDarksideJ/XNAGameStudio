#region File Description
//-----------------------------------------------------------------------------
// CustomImporterContext.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework.Content.Pipeline;
#endregion

namespace CompileEffect
{
    /// <summary>
    /// Custom context object for running Content Pipeline importers.
    /// </summary>
    class CustomImporterContext : ContentImporterContext
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CustomImporterContext(ContentBuildLogger logger)
        {
            this.logger = logger;
        }


        /// <summary>
        /// Gets a logger for reporting content build messages and warnings.
        /// </summary>
        public override ContentBuildLogger Logger
        {
            get { return logger; }
        }

        ContentBuildLogger logger;


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
        /// Records any additional input files that were read by the importer.
        /// </summary>
        public override void AddDependency(string filename)
        { 
        }
    }
}
