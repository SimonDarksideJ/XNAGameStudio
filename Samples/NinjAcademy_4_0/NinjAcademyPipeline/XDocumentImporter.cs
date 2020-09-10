#region File Description
//-----------------------------------------------------------------------------
// ConfigurationImporter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.Xml.Linq;


#endregion

namespace NinjAcademy.Pipeline
{
    /// <summary>
    /// Simply returns an XDocument from a specified XML file.
    /// </summary>
    [ContentImporter(".xml", DisplayName = "XDocument Importer", 
        DefaultProcessor = "NinjAcademy Configuration Processor")]
    public class XDocumentImporter : ContentImporter<XDocument>
    {
        public override XDocument Import(string filename, ContentImporterContext context)
        {
            return XDocument.Load(filename);
        }
    }
}
