#region File Description
//-----------------------------------------------------------------------------
// AnimationProcessor.cs
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
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.Xml.Linq;


#endregion

namespace NinjAcademy.Pipeline
{
    /// <summary>
    /// Takes an animation definition XML and turns it into a dictionary from animation aliases to animation 
    /// instances.
    /// </summary>
    [ContentProcessor(DisplayName = "NinjAcademy Animation Processor")]
    public class AnimationProcessor : ContentProcessor<XDocument, AnimationStore>
    {
        public override AnimationStore Process(XDocument input, ContentProcessorContext context)
        {
            Dictionary<string, Animation> animations = new Dictionary<string, Animation>();

            var definitions = input.Document.Descendants("Definition");

            // Loop over all definitions in the XML
            foreach (XElement animationDefinition in definitions)
            {
                // Get the name of the animation
                string animationAlias = animationDefinition.Attribute("Alias").Value;
                string animationSheetPath = animationDefinition.Attribute("SheetName").Value;

                // Get the frame size (width & height)
                Point frameDimensions = new Point();
                frameDimensions.X = int.Parse(animationDefinition.Attribute("FrameWidth").Value);
                frameDimensions.Y = int.Parse(animationDefinition.Attribute("FrameHeight").Value);

                // Get the frame size (width & height)
                Vector2 visualCenter = Vector2.Zero;
                visualCenter.X = float.Parse(animationDefinition.Attribute("VisualCenterX").Value);
                visualCenter.Y = float.Parse(animationDefinition.Attribute("VisualCenterY").Value);

                // Get the frames sheet dimensions
                Point rowsAndColumns = new Point();
                rowsAndColumns.X = int.Parse(animationDefinition.Attribute("SheetColumns").Value);
                rowsAndColumns.Y = int.Parse(animationDefinition.Attribute("SheetRows").Value);

                // Get whether or not the animation is cyclic
                bool isCyclic = bool.Parse(animationDefinition.Attribute("Cyclic").Value);

                Animation animation = 
                    new Animation(animationSheetPath, frameDimensions, rowsAndColumns, visualCenter, isCyclic);

                // Get information regarding the animation's speed (otherwise a frame changes each update)
                animation.SetFrameInterval(TimeSpan.FromMilliseconds(
                    double.Parse(animationDefinition.Attribute("FrameMillisecondInterval").Value)));

                animations.Add(animationAlias, animation);
            }

            return new AnimationStore() { Animations = animations };            
        }
    }
}