#region File Description
//-----------------------------------------------------------------------------
// Program.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#endregion

namespace CompileEffect
{
    /// <summary>
    /// Commandline utility compiles .fx effect sources file into binary files that can be consumed by XNA Game Studio. 
    /// </summary>
    class Program
    {
        static int Main(string[] args)
        {
            // Make sure we have the right number of commandline arguments.
            if (args.Length != 4)
            {
                Console.Error.WriteLine("Usage: CompileEffect <targetPlatform> <targetProfile> <input.fx> <output.bin>");
                return 1;
            }

            // Parse the commandline arguments.
            TargetPlatform targetPlatform;

            if (!Enum.TryParse(args[0], true, out targetPlatform))
            {
                Console.Error.WriteLine("Invalid target platform {0}. Valid options are {1}.", args[0], GetEnumValues<TargetPlatform>());
                return 1;
            }

            GraphicsProfile targetProfile;

            if (!Enum.TryParse(args[1], true, out targetProfile))
            {
                Console.Error.WriteLine("Invalid target profile {0}. Valid options are {1}.", args[1], GetEnumValues<GraphicsProfile>());
                return 1;
            }

            string inputFilename = args[2];
            string outputFilename = args[3];

            try
            {
                Console.WriteLine("Compiling {0} -> {1} for {2}, {3}", Path.GetFileName(inputFilename), outputFilename, targetPlatform, targetProfile);

                ContentBuildLogger logger = new CustomLogger();

                // Import the effect source code.
                EffectImporter importer = new EffectImporter();
                ContentImporterContext importerContext = new CustomImporterContext(logger);
                EffectContent sourceEffect = importer.Import(inputFilename, importerContext);

                // Compile the effect.
                EffectProcessor processor = new EffectProcessor();
                ContentProcessorContext processorContext = new CustomProcessorContext(targetPlatform, targetProfile, logger);
                CompiledEffectContent compiledEffect = processor.Process(sourceEffect, processorContext);

                // Write out the compiled effect code.
                File.WriteAllBytes(outputFilename, compiledEffect.GetEffectCode());
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error: {0}", e.Message);
                return 1;
            }

            return 0;
        }


        /// <summary>
        /// Helper returns a comma separated list of all the possible values of an enum.
        /// </summary>
        static string GetEnumValues<T>()
        {
            T[] values = (T[])Enum.GetValues(typeof(T));

            return string.Join(", ", from t in values select t.ToString());
        }
    }
}
