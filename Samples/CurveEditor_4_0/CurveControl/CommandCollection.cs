#region File Description
//-----------------------------------------------------------------------------
// CommandCollection.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

//-----------------------------------------------------------------------------
// CommandCollection.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;

#endregion



namespace Xna.Tools
{
    /// <summary>
    /// This class manages ICommands for Undo/Redo.
    /// Also, CommandQueue works like ICommand. So you can capture multiple
    /// commands as one command.
    /// </summary>
    public class CommandCollection : ICommand, ICollection<ICommand>
    {
        #region Properties
        /// <summary>
        /// It returns true if it can process undo; otherwise it returns false.
        /// </summary>
        public bool CanUndo { get { return commandIndex > 0; } }

        /// <summary>
        /// It returns true if it can process redo; otherwise it returns false.
        /// </summary>
        public bool CanRedo { get { return commandIndex < commands.Count; } }

        /// <summary>
        /// Return number of commands in this queue.
        /// </summary>
        public int Count { get { return commands.Count; } }

        /// <summary>
        /// Current command index.
        /// </summary>
        public int Index { get { return commandIndex; } }

        /// <summary>
        /// Gets the element at the specfied index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The element at the specfied index.</</returns>
        public ICommand this[int index] { get { return commands[index]; } }
        #endregion

        #region Public Methods

        /// <summary>
        /// Add command to queue.
        /// </summary>
        /// <param name="command"></param>
        public void Add(ICommand item)
        {
            // Discard rest of commands from commandIndex.
            commands.RemoveRange(commandIndex, commands.Count - commandIndex);

            // Add command to commands.
            commands.Add(item);
            commandIndex = commands.Count;
        }

        /// <summary>
        /// Undo command.
        /// </summary>
        public bool Undo()
        {
            if (!CanUndo) return false;
            commands[--commandIndex].Unexecute();
            return true;
        }

        /// <summary>
        /// Redo command.
        /// </summary>
        public bool Redo()
        {
            if (!CanRedo) return false;
            commands[commandIndex++].Execute();
            return true;
        }

        #region ICommand Members
        public void Execute()
        {
            // Execute all commands.
            foreach (ICommand command in commands)
                command.Execute();
        }

        public void Unexecute()
        {
            // Unexecute all commands.
            for (int i = commands.Count - 1; i >= 0; --i)
                commands[i].Unexecute();
        }
        #endregion

        #endregion

        #region Protected members

        /// <summary>
        /// For store commands.
        /// </summary>
        List<ICommand> commands = new List<ICommand>();

        /// <summary>
        /// Current command index.
        /// </summary>
        int commandIndex;

        #endregion

        #region ICollection<ICommand> Members

        public void Clear()
        {
            commands.Clear();
            commandIndex = 0;
        }

        public bool Contains(ICommand item)
        {
            return commands.Contains(item);
        }

        public void CopyTo(ICommand[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(ICommand item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<ICommand> Members

        public IEnumerator<ICommand> GetEnumerator()
        {
            return commands.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return commands.GetEnumerator();
        }

        #endregion
    }
}
