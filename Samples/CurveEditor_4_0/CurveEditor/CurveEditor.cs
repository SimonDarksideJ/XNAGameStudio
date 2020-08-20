//-----------------------------------------------------------------------------
// CurveEditor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;

using Microsoft.Xna.Framework;

namespace Xna.Tools
{
    public partial class CurveEditor : Form
    {
        public CurveEditor()
        {
            InitializeComponent();

            if (Site != null) Site.Container.Add(curveControl);
        }

        #region Internal Methods
        internal void UpdateCurveItems(IList newItems, IList selection)
        {
            disableUIEvents++;
            curveListView.BeginUpdate();

            ArrayList oldItems = new ArrayList(curveListView.Items);
            curveListView.Items.Clear();

            // Add new items.
            foreach (ListViewItem item in newItems)
            {
                curveListView.Items.Add(item);

                if (!oldItems.Contains(item))
                    curveControl.Curves.Add(CurveFileInfo.GetCurve(item));
            }

            // Remove items from Curve control that not exist new items.
            foreach (ListViewItem item in oldItems)
            {
                if (!curveListView.Items.Contains(item))
                    curveControl.Curves.Remove(CurveFileInfo.GetCurve(item));
            }

            ApplySelection(selection);

            curveListView.EndUpdate();
            disableUIEvents--;
        }

        internal void ApplySelection(IList selection)
        {
            disableUIEvents++;
            curveListView.BeginUpdate();

            // Update selection
            curveListView.SelectedItems.Clear();
            foreach (ListViewItem item in selection)
                item.Selected = true;

            curveListView.EndUpdate();
            disableUIEvents--;
        }
        #endregion

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            foreach ( ListViewItem item in curveListView.Items )
            {
                EditCurve curve = CurveFileInfo.GetCurve(item);
                if ( curve.Dirty )
                {
                    DialogResult dr = MessageBox.Show( 
                        String.Format( CurveEditorResources.SaveMessage, curve.Name ),
                        CurveEditorResources.CurveEditorTitle,
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);

                    if ( dr == DialogResult.Yes)
                        dr = SaveCurve(item, false);

                    if (dr == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        break;
                    }
                }
            }

            base.OnFormClosing(e);
        }

        
        #region Event handle methods
        private void curveListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (disableUIEvents > 0) return;

            // Label edit has been canceled.
            if (String.IsNullOrEmpty(e.Label)) return;

            //
            commandHistory.BeginRecordCommands();

            curveControl.BeginUpdate();
            string newName = EnsureUniqueName(e.Label);

            e.CancelEdit = String.Compare(newName, e.Label) != 0;

            curveListView.Items[e.Item].Text = newName;
            EditCurve curve = CurveFileInfo.GetCurve(curveListView.Items[e.Item]);
            curve.Name = newName;
            curveControl.EndUpdate();

            commandHistory.EndRecordCommands();
        }

        private void CurveEditor_Load(object sender, EventArgs e)
        {
            commandHistory = CommandHistory.EnsureHasService(Site);
        }

        private void curveListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                RemoveSelectedCurves();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateCurve();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;

            foreach (string filename in openFileDialog1.FileNames)
            {
                string name = 
                    EnsureUniqueName(Path.GetFileNameWithoutExtension(filename));
                EditCurve editCurve = EditCurve.LoadFromFile(filename, name,
                                                    NextCurveColor(), commandHistory);
                ListViewItem item = CreateCurve(editCurve, name);
                CurveFileInfo.AssignFilename(item, filename);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveCurves(false);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveCurves(true);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
         }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (HelpAbout about = new HelpAbout())
            {
                about.ShowDialog();
            }
        }

        private void curveListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (disableUIEvents > 0) return;
            UpdateCurveEditables();
            curveControl.Invalidate(true);
        }

        private void curveListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (disableUIEvents > 0) return;
            CurveFileInfo.GetCurve(e.Item).Visible = e.Item.Checked;
            curveControl.Invalidate(true);
        }

        private void editCurve_StateChanged(object sender, EventArgs e)
        {
            disableUIEvents++;
            curveListView.BeginUpdate();
            EditCurve curve = sender as EditCurve;
            ListViewItem item = idToItemMap[curve.Id];

            item.Text = curve.Name;

            disableUIEvents--;
            curveListView.EndUpdate();
        }
        #endregion

        #region Private methods

        private System.Drawing.Color NextCurveColor()
        {
            System.Drawing.Color result = curveColors[curveColorIndex++];
            if (curveColorIndex >= curveColors.Length) curveColorIndex = 0;
            return result;
        }

        private ListViewItem CreateCurve(EditCurve editCurve, string name)
        {
            editCurve.Id = GenerateUniqueCurveId();
            editCurve.StateChanged += new EventHandler(editCurve_StateChanged);

            ListViewItem item = new ListViewItem(name);
            item.Checked = true;
            CurveFileInfo.AssignCurve(item, editCurve);

            idToItemMap.Add(editCurve.Id, item);

            ArrayList oldItems = new ArrayList(curveListView.Items);
            ArrayList oldSelection = new ArrayList(curveListView.SelectedItems);
            ArrayList newItems = new ArrayList(curveListView.Items);
            ListViewItem[] newSelection = { item };
            newItems.Add(item);

            //curveListView.Items.Add(item);
            //curveListView.SelectedItems.Clear();
            //item.Selected = true;
            commandHistory.BeginRecordCommands();

            commandHistory.Do( new CurveAddRemoveCommand(this, oldItems, newItems,
                                                        oldSelection, newSelection));

            UpdateCurveEditables();

            commandHistory.EndRecordCommands();

            return item;
        }

        private ListViewItem CreateCurve()
        {
            string name = EnsureUniqueName("Curve");
            Curve curve = new Curve();
            curve.Keys.Add(new CurveKey(0, 0));

            EditCurve editCurve =
                        new EditCurve(name, NextCurveColor(), curve, commandHistory);
            editCurve.Dirty = true;
            return CreateCurve(editCurve, name);
        }

        private void UpdateCurveEditables()
        {
            UpdateCurves(delegate(ListViewItem item, EditCurve curve)
            {
                curve.Editable = item.Selected;
            });
        }

        private void RemoveSelectedCurves()
        {
            commandHistory.BeginRecordCommands();

            ArrayList newItems = new ArrayList();
            ArrayList newSelection = new ArrayList();
            foreach (ListViewItem item in curveListView.Items)
            {
                if (!item.Selected)
                {
                    newItems.Add(item);
                    if (newSelection.Count == 0 )newSelection.Add(item);
                }
            }

            ArrayList oldItems = new ArrayList(curveListView.Items);
            ArrayList oldSelection = new ArrayList(curveListView.SelectedItems);

            commandHistory.Do( new CurveAddRemoveCommand(this, oldItems, newItems,
                                                        oldSelection, newSelection));
            UpdateCurveEditables();
            commandHistory.EndRecordCommands();
        }

        private string EnsureUniqueName(string candidateName)
        {
            string name =
                String.IsNullOrEmpty(candidateName) ? "Empty" : candidateName;

            int num = 1;
            bool found = false;
            do
            {
                found = false;
                foreach (ListViewItem item in curveListView.Items)
                {
                    int result = String.Compare(
                                item.Text, name, true, CultureInfo.InvariantCulture);
                    if (result == 0)
                    {
                        found = true;
                        name = String.Format("{0}#{1}", candidateName, num++);
                        break;
                    }
                }

            } while (found);

            return name;
        }


        /// <summary>
        /// Update curves.
        /// </summary>
        /// <param name="callback"></param>
        private void UpdateCurves(UpdateCurveDelegate callback)
        {
            curveControl.BeginUpdate();
            foreach (ListViewItem item in curveListView.Items)
                callback(item, CurveFileInfo.GetCurve(item));
            curveControl.EndUpdate();
        }

        private DialogResult SaveCurve(ListViewItem item, bool saveAs)
        {
            string filename = CurveFileInfo.GetFilename(item);
            if (String.IsNullOrEmpty(filename) || saveAs)
            {
                saveFileDialog1.Title = saveAs ? "Save Curve As" : "Save Curve";
                saveFileDialog1.FileName = Path.ChangeExtension(item.Text, ".xml");

                DialogResult dr = saveFileDialog1.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    filename = saveFileDialog1.FileName;
                    CurveFileInfo.AssignFilename(item, filename);
                }

                if (dr == DialogResult.Cancel) return dr;
            }

            // Save to file.
            if (!String.IsNullOrEmpty(filename))
                CurveFileInfo.Save(item, filename);

            return DialogResult.OK;
        }

        private void SaveCurves(bool saveAs)
        {
            // Save current curves.
            foreach (ListViewItem item in curveListView.SelectedItems)
            {
                if (SaveCurve(item, saveAs) == DialogResult.Cancel) break;
            }
        }

        /// <summary>
        /// Generate Unique Id for EditCurve
        /// We need to assign unique Id for each EditCurve for undo/redo.
        /// </summary>
        /// <returns></returns>
        private long GenerateUniqueCurveId()
        {
            return curveId++;
        }

        delegate void UpdateCurveDelegate(ListViewItem item, EditCurve curve);

        #endregion

        private long curveId;

        private CommandHistory commandHistory;

        private Dictionary<long, ListViewItem> idToItemMap =
                                                new Dictionary<long, ListViewItem>();

        private int disableUIEvents;

        private System.Drawing.Color[] curveColors = { System.Drawing.Color.Red, System.Drawing.Color.Green, System.Drawing.Color.Blue };
        private int curveColorIndex;

        class CurveFileInfo
        {
            public EditCurve Curve;
            public string Filename;

            public static void AssignCurve( ListViewItem item, EditCurve curve )
            {
                CurveFileInfo cfi = EnsureFileInfo(item);
                cfi.Curve = curve;
            }

            public static void AssignFilename(ListViewItem item, string filename)
            {
                CurveFileInfo cfi = EnsureFileInfo(item);
                cfi.Filename = filename;
            }

            public static EditCurve GetCurve(ListViewItem item)
            {
                return EnsureFileInfo(item).Curve;
            }

            public static string GetFilename(ListViewItem item)
            {
                return EnsureFileInfo(item).Filename;
            }

            public static void Save(ListViewItem item, string filename)
            {
                GetCurve(item).Save(filename);
            }

            private static CurveFileInfo EnsureFileInfo(ListViewItem item)
            {
                if (item.Tag == null)
                    item.Tag = new CurveFileInfo();

                return item.Tag as CurveFileInfo;
            }
        }

    }
}