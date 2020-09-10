//-----------------------------------------------------------------------------
// CurveEditorCommands.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Xna.Tools
{

    public class CurveAddRemoveCommand : ICommand
    {
        CurveEditor editor;

        ArrayList oldSelection;
        ArrayList newSelection;
        ArrayList newItems;
        ArrayList oldItems;

        public CurveAddRemoveCommand(CurveEditor editor,
            IList oldItems, IList newItems, IList oldSelection, IList newSelection)
        {
            // Copy selection list.
            this.editor = editor;
            this.oldItems = new ArrayList(oldItems);
            this.newItems = new ArrayList(newItems);
            this.oldSelection = new ArrayList(oldSelection);
            this.newSelection = new ArrayList(newSelection);
        }

        public void Execute()
        {
            editor.UpdateCurveItems(newItems, newSelection);
        }

        public void Unexecute()
        {
            editor.UpdateCurveItems(oldItems, oldSelection);
        }
    }

}
