#region File Description
//-----------------------------------------------------------------------------
// CommandHistory.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

//-----------------------------------------------------------------------------
// CommandHistory.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#region Using Statements
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Text;

#endregion




namespace Xna.Tools
{
    /// <summary>
    /// This class provides Undo/Redo feature.
    /// </summary>
    public class CommandHistory
    {
        #region Properties
        /// <summary>
        /// Changed event that fired when command history changed.
        /// </summary>
        public event EventHandler Changed;

        /// <summary>
        /// It returns true if it can process undo; otherwise it returns false.
        /// </summary>
        public bool CanUndo { get { return commands.CanUndo; } }

        /// <summary>
        /// It returns true if it can process redo; otherwise it returns false.
        /// </summary>
        public bool CanRedo { get { return commands.CanRedo; } }
        #endregion

        #region Public Methods

        /// <summary>
        /// Make sure CommandHistory added as a service to given site.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <remarks>You have to make sure added IServiceContainer to this
        /// serviceProvider.</remarks>
        public static CommandHistory EnsureHasService(IServiceProvider serviceProvider)
        {
            CommandHistory result = null;
            IServiceProvider sp = serviceProvider;
            if (sp != null)
            {
                // Add this CommandHistory service if given service
                // doesn't contains it.
                result = sp.GetService(typeof(CommandHistory)) as CommandHistory;
                if (result == null)
                {
                    // If there are no service, added new instance.
                    IServiceContainer s = sp.GetService(typeof(IServiceContainer))
                        as IServiceContainer;

                    if (s == null)
                    {
                        throw new InvalidOperationException(
                            CurveControlResources.RequireServiceContainer);
                    }

                    result = new CommandHistory();
                    s.AddService(typeof(CommandHistory), result);
                }
            }
            else
            {
                // If they don't have ISite, returns static instance.
                if (staticCommandHistory == null)
                    staticCommandHistory = new CommandHistory();

                result = staticCommandHistory;
            }

            return result;
        }

        /// <summary>
        /// Execute given command and added to history.
        /// </summary>
        /// <param name="command"></param>
        public void Do(ICommand command)
        {
            if (command == null)
                throw new ArgumentNullException("command");

            command.Execute();
            Add(command);
        }

        /// <summary>
        /// Add command to history.
        /// </summary>
        /// <param name="command"></param>
        public void Add(ICommand command)
        {
            DebugPrintCommand("Add", command);

            // Add given command to commands or recordingCommands.
            if (recordingCommands != null)
            {
                recordingCommands.Add(command);
            }
            else
            {
                commands.Add(command);
                if (Changed != null) Changed(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Undo command.
        /// </summary>
        public void Undo()
        {
            if (recordingNestCount != 0)
                throw new InvalidOperationException(String.Format(
                    CurveControlResources.CantExecuteCommandHistory, "Undo"));

            DebugPrintCommand("Undo", -1);

            commands.Undo();
            if (Changed != null) Changed(this, EventArgs.Empty);
        }

        /// <summary>
        /// Redo command.
        /// </summary>
        public void Redo()
        {
            if (recordingNestCount != 0)
                throw new InvalidOperationException(String.Format(
                    CurveControlResources.CantExecuteCommandHistory, "Redo"));

            DebugPrintCommand("Redo", 0);

            commands.Redo();
            if (Changed != null) Changed(this, EventArgs.Empty);
        }

        /// <summary>
        /// Record multiple commands as one command.
        /// </summary>
        public void BeginRecordCommands()
        {
            if (recordingNestCount++ == 0)
                recordingCommands = new CommandCollection();
        }

        /// <summary>
        /// Stop recording commands and added as a command.
        /// </summary>
        public void EndRecordCommands()
        {
            if (--recordingNestCount != 0) return;

            // Added recorded commands to commands.
            // If recording commands recored one command, we just add that command to
            // main commands.
            if (recordingCommands.Count != 0)
            {
                if (recordingCommands.Count == 1)
                    commands.Add(recordingCommands[0]);
                else
                    commands.Add(recordingCommands);

                if (Changed != null) Changed(this, EventArgs.Empty);
            }

            recordingCommands = null;
        }

        #endregion

        #region Private Members

        /// <summary>
        /// Main CommandQueue.
        /// </summary>
        CommandCollection commands = new CommandCollection();

        /// <summary>
        /// Sub CommandQueue that uses for command recoording.
        /// </summary>
        CommandCollection recordingCommands = null;

        /// <summary>
        /// For keep tracking nested call of Begin/EndRecoord method.
        /// </summary>
        int recordingNestCount = 0;

        /// <summary>
        /// Static command history that only avilable when user doesn't provide
        /// ISite object.
        /// </summary>
        static CommandHistory staticCommandHistory = null;

        #endregion

        #region Debug Code
        [Conditional("DEBUG")]
        static void DebugPrintCommand(string prefix, ICommand command)
        {
            System.Diagnostics.Debugger.Log(0, "CmdHistory",
                String.Format("{0}:{1}\n", prefix, command.ToString()));

            CommandCollection cq = command as CommandCollection;
            if (cq != null)
            {
                for (int i = 0; i < cq.Count; ++i)
                {
                    System.Diagnostics.Debugger.Log(0, "CmdHistory",
                        String.Format("    [{0}]:{1}\n", i, cq[i].ToString()));
                }
            }
        }

        [Conditional("DEBUG")]
        private void DebugPrintCommand(string prefix, int offset)
        {
            DebugPrintCommand(prefix, commands[commands.Index + offset]);
        }
        #endregion
    }
}
