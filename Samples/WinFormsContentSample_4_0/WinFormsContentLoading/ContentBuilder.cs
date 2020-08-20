#region File Description
//-----------------------------------------------------------------------------
// ContentBuilder.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
#endregion

namespace WinFormsContentLoading
{
    /// <summary>
    /// This class wraps the MSBuild functionality needed to build XNA Framework
    /// content dynamically at runtime. It creates a temporary MSBuild project
    /// in memory, and adds whatever content files you choose to this project.
    /// It then builds the project, which will create compiled .xnb content files
    /// in a temporary directory. After the build finishes, you can use a regular
    /// ContentManager to load these temporary .xnb files in the usual way.
    /// </summary>
    class ContentBuilder : IDisposable
    {
        #region Fields


        // What importers or processors should we load?
        const string xnaVersion = ", Version=4.0.0.0, PublicKeyToken=842cf8be1de50553";

        static string[] pipelineAssemblies =
        {
            "Microsoft.Xna.Framework.Content.Pipeline.FBXImporter" + xnaVersion,
            "Microsoft.Xna.Framework.Content.Pipeline.XImporter" + xnaVersion,
            "Microsoft.Xna.Framework.Content.Pipeline.TextureImporter" + xnaVersion,
            "Microsoft.Xna.Framework.Content.Pipeline.EffectImporter" + xnaVersion,

            // If you want to use custom importers or processors from
            // a Content Pipeline Extension Library, add them here.
            //
            // If your extension DLL is installed in the GAC, you should refer to it by assembly
            // name, eg. "MyPipelineExtension, Version=1.0.0.0, PublicKeyToken=1234567812345678".
            //
            // If the extension DLL is not in the GAC, you should refer to it by
            // file path, eg. "c:/MyProject/bin/MyPipelineExtension.dll".
        };


        // MSBuild objects used to dynamically build content.
        Project buildProject;
        ProjectRootElement projectRootElement;
        BuildParameters buildParameters;
        List<ProjectItem> projectItems = new List<ProjectItem>();
        ErrorLogger errorLogger;


        // Temporary directories used by the content build.
        string buildDirectory;
        string processDirectory;
        string baseDirectory;


        // Generate unique directory names if there is more than one ContentBuilder.
        static int directorySalt;


        // Have we been disposed?
        bool isDisposed;


        #endregion

        #region Properties


        /// <summary>
        /// Gets the output directory, which will contain the generated .xnb files.
        /// </summary>
        public string OutputDirectory
        {
            get { return Path.Combine(buildDirectory, "bin/Content"); }
        }


        #endregion

        #region Initialization


        /// <summary>
        /// Creates a new content builder.
        /// </summary>
        public ContentBuilder()
        {
            CreateTempDirectory();
            CreateBuildProject();
        }


        /// <summary>
        /// Finalizes the content builder.
        /// </summary>
        ~ContentBuilder()
        {
            Dispose(false);
        }


        /// <summary>
        /// Disposes the content builder when it is no longer required.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Implements the standard .NET IDisposable pattern.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                isDisposed = true;

                DeleteTempDirectory();
            }
        }


        #endregion

        #region MSBuild


        /// <summary>
        /// Creates a temporary MSBuild content project in memory.
        /// </summary>
        void CreateBuildProject()
        {
            string projectPath = Path.Combine(buildDirectory, "content.contentproj");
            string outputPath = Path.Combine(buildDirectory, "bin");

            // Create the build project.
            projectRootElement = ProjectRootElement.Create(projectPath);

            // Include the standard targets file that defines how to build XNA Framework content.
            projectRootElement.AddImport("$(MSBuildExtensionsPath)\\Microsoft\\XNA Game Studio\\" +
                                         "v4.0\\Microsoft.Xna.GameStudio.ContentPipeline.targets");

            buildProject = new Project(projectRootElement);

            buildProject.SetProperty("XnaPlatform", "Windows");
            buildProject.SetProperty("XnaProfile", "Reach");
            buildProject.SetProperty("XnaFrameworkVersion", "v4.0");
            buildProject.SetProperty("Configuration", "Release");
            buildProject.SetProperty("OutputPath", outputPath);

            // Register any custom importers or processors.
            foreach (string pipelineAssembly in pipelineAssemblies)
            {
                buildProject.AddItem("Reference", pipelineAssembly);
            }

            // Hook up our custom error logger.
            errorLogger = new ErrorLogger();

            buildParameters = new BuildParameters(ProjectCollection.GlobalProjectCollection);
            buildParameters.Loggers = new ILogger[] { errorLogger };
        }


        /// <summary>
        /// Adds a new content file to the MSBuild project. The importer and
        /// processor are optional: if you leave the importer null, it will
        /// be autodetected based on the file extension, and if you leave the
        /// processor null, data will be passed through without any processing.
        /// </summary>
        public void Add(string filename, string name, string importer, string processor)
        {
            ProjectItem item = buildProject.AddItem("Compile", filename)[0];

            item.SetMetadataValue("Link", Path.GetFileName(filename));
            item.SetMetadataValue("Name", name);

            if (!string.IsNullOrEmpty(importer))
                item.SetMetadataValue("Importer", importer);

            if (!string.IsNullOrEmpty(processor))
                item.SetMetadataValue("Processor", processor);

            projectItems.Add(item);
        }


        /// <summary>
        /// Removes all content files from the MSBuild project.
        /// </summary>
        public void Clear()
        {
            buildProject.RemoveItems(projectItems);

            projectItems.Clear();
        }


        /// <summary>
        /// Builds all the content files which have been added to the project,
        /// dynamically creating .xnb files in the OutputDirectory.
        /// Returns an error message if the build fails.
        /// </summary>
        public string Build()
        {
            // Clear any previous errors.
            errorLogger.Errors.Clear();

            // Create and submit a new asynchronous build request.
            BuildManager.DefaultBuildManager.BeginBuild(buildParameters);
            
            BuildRequestData request = new BuildRequestData(buildProject.CreateProjectInstance(), new string[0]);
            BuildSubmission submission = BuildManager.DefaultBuildManager.PendBuildRequest(request);

            submission.ExecuteAsync(null, null);

            // Wait for the build to finish.
            submission.WaitHandle.WaitOne();

            BuildManager.DefaultBuildManager.EndBuild();

            // If the build failed, return an error string.
            if (submission.BuildResult.OverallResult == BuildResultCode.Failure)
            {
                return string.Join("\n", errorLogger.Errors.ToArray());
            }

            return null;
        }


        #endregion

        #region Temp Directories


        /// <summary>
        /// Creates a temporary directory in which to build content.
        /// </summary>
        void CreateTempDirectory()
        {
            // Start with a standard base name:
            //
            //  %temp%\WinFormsContentLoading.ContentBuilder

            baseDirectory = Path.Combine(Path.GetTempPath(), GetType().FullName);

            // Include our process ID, in case there is more than
            // one copy of the program running at the same time:
            //
            //  %temp%\WinFormsContentLoading.ContentBuilder\<ProcessId>

            int processId = Process.GetCurrentProcess().Id;

            processDirectory = Path.Combine(baseDirectory, processId.ToString());

            // Include a salt value, in case the program
            // creates more than one ContentBuilder instance:
            //
            //  %temp%\WinFormsContentLoading.ContentBuilder\<ProcessId>\<Salt>

            directorySalt++;

            buildDirectory = Path.Combine(processDirectory, directorySalt.ToString());

            // Create our temporary directory.
            Directory.CreateDirectory(buildDirectory);

            PurgeStaleTempDirectories();
        }


        /// <summary>
        /// Deletes our temporary directory when we are finished with it.
        /// </summary>
        void DeleteTempDirectory()
        {
            Directory.Delete(buildDirectory, true);

            // If there are no other instances of ContentBuilder still using their
            // own temp directories, we can delete the process directory as well.
            if (Directory.GetDirectories(processDirectory).Length == 0)
            {
                Directory.Delete(processDirectory);

                // If there are no other copies of the program still using their
                // own temp directories, we can delete the base directory as well.
                if (Directory.GetDirectories(baseDirectory).Length == 0)
                {
                    Directory.Delete(baseDirectory);
                }
            }
        }


        /// <summary>
        /// Ideally, we want to delete our temp directory when we are finished using
        /// it. The DeleteTempDirectory method (called by whichever happens first out
        /// of Dispose or our finalizer) does exactly that. Trouble is, sometimes
        /// these cleanup methods may never execute. For instance if the program
        /// crashes, or is halted using the debugger, we never get a chance to do
        /// our deleting. The next time we start up, this method checks for any temp
        /// directories that were left over by previous runs which failed to shut
        /// down cleanly. This makes sure these orphaned directories will not just
        /// be left lying around forever.
        /// </summary>
        void PurgeStaleTempDirectories()
        {
            // Check all subdirectories of our base location.
            foreach (string directory in Directory.GetDirectories(baseDirectory))
            {
                // The subdirectory name is the ID of the process which created it.
                int processId;

                if (int.TryParse(Path.GetFileName(directory), out processId))
                {
                    try
                    {
                        // Is the creator process still running?
                        Process.GetProcessById(processId);
                    }
                    catch (ArgumentException)
                    {
                        // If the process is gone, we can delete its temp directory.
                        Directory.Delete(directory, true);
                    }
                }
            }
        }

        
        #endregion
    }
}
