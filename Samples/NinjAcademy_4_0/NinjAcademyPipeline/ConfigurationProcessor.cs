#region File Description
//-----------------------------------------------------------------------------
// ConfigurationProcessor.cs
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
    /// Processes the game's configuration XML and returns a configuration object.
    /// </summary>
    [ContentProcessor(DisplayName = "NinjAcademy Configuration Processor")]
    public class ConfigurationProcessor : ContentProcessor<XDocument, GameConfiguration>
    {
        #region Fields


        const int TargetAmount = 3;


        #endregion


        /// <summary>
        /// Translates a configuration XML into a configuration object.
        /// </summary>
        /// <param name="input">Configuration XML.</param>
        /// <param name="context">Context for the context processing operation.</param>
        /// <returns>A <see cref="GameConfiguration"/> object matching the configuration XML processed.</returns>
        public override GameConfiguration Process(XDocument input, ContentProcessorContext context)
        {
            GameConfiguration result = new GameConfiguration();

            XElement rootElement = input.Root;

            XElement GeneralSettings = rootElement.Element("General");
            result.PlayerLives = int.Parse(GeneralSettings.Attribute("PlayerLife").Value);
            result.PointsPerBamboo = int.Parse(GeneralSettings.Attribute("PointsPerBamboo").Value);
            result.PointsPerTarget = int.Parse(GeneralSettings.Attribute("PointsPerTarget").Value);
            result.PointsPerGoldTarget = int.Parse(GeneralSettings.Attribute("PointsPerGoldTarget").Value);

            // Perform some sanity checks on the configuration
            if (result.PlayerLives <= 0)
            {
                throw new InvalidContentException("Player must have 1 or more lives.");
            }

            List<GamePhase> gamePhases = new List<GamePhase>();

            XElement gamePhasesElement = rootElement.Element("GamePhases");

            foreach (XElement phaseElement in gamePhasesElement.Elements("Phase"))
            {
                gamePhases.Add(ParsePhaseElement(phaseElement));
            }

            result.Phases = gamePhases;

            return result;
        }

        /// <summary>
        /// Extracts data from a game phase element.
        /// </summary>
        /// <param name="phaseElement">XML element depicting the game phase.</param>
        /// <returns><see cref="GamePhase"/> instance containing the data parsed from 
        /// <paramref name="phaseElement"/></returns>
        private GamePhase ParsePhaseElement(XElement phaseElement)
        {
            GamePhase result = new GamePhase();

            // Get phase duration
            result.Duration = TimeSpan.FromSeconds(double.Parse(phaseElement.Attribute("Duration").Value));

            // Get gold target probability
            result.GoldTargetProbablity = 
                double.Parse(phaseElement.Element("Targets").Attribute("GoldProbability").Value);

            // Get Target related information
            var targetElements = phaseElement.Element("Targets").Elements("Target");

            if (targetElements.Count() != TargetAmount)
            {
                throw new InvalidContentException(String.Format("There must be exactly {0} \"Target\" elements.",
                    TargetAmount));
            }

            result.TargetAppearanceIntervals = new TimeSpan[TargetAmount];
            result.TargetAppearanceProbabilities = new double[TargetAmount];

            int index;

            for (index = 0; index < TargetAmount; index++)
            {
                result.TargetAppearanceIntervals[index] =
                    TimeSpan.FromSeconds(double.Parse(targetElements.ElementAt(index).Attribute("Interval").Value));
                result.TargetAppearanceProbabilities[index] =
                    double.Parse(targetElements.ElementAt(index).Attribute("Probability").Value);

                EnsureLegalProbability(result.TargetAppearanceProbabilities[index]);
            }

            // Get bamboo related information
            XElement bambooElement = phaseElement.Element("Bamboo");

            result.BambooAppearanceInterval =
                TimeSpan.FromSeconds(double.Parse(bambooElement.Attribute("Interval").Value));
            result.BambooAppearanceProbablity = double.Parse(bambooElement.Attribute("Probability").Value);

            EnsureLegalProbability(result.BambooAppearanceProbablity);

            // Get dynamite related information
            XElement dynamiteElement = phaseElement.Element("Dynamite");

            result.DynamiteAppearanceInterval =
                TimeSpan.FromSeconds(double.Parse(dynamiteElement.Attribute("Interval").Value));
            result.DynamiteAppearanceProbablity = double.Parse(dynamiteElement.Attribute("Probability").Value);

            EnsureLegalProbability(result.DynamiteAppearanceProbablity);

            var dynamiteAmountProbabilityElements = dynamiteElement.Elements("AmountProbability");

            // Used to make sure dynamite probabilities sum up to 1
            double probabilitySum = 0;

            result.DynamiteAmountProbabilities = new double[dynamiteAmountProbabilityElements.Count()];

            index = 0;
            foreach (XElement amountProbabilityElement in dynamiteAmountProbabilityElements)
            {
                double probability = double.Parse(amountProbabilityElement.Attribute("Probability").Value);

                EnsureLegalProbability(probability);

                probabilitySum += probability;

                result.DynamiteAmountProbabilities[index++] = probability;
            }

            if (probabilitySum != 1)
            {
                throw new InvalidContentException("Dynamite stick amount probabilities must sum up to 1.");
            }

            return result;
        }

        /// <summary>
        /// Throws an exception if the supplied value is not between 0 and 1 inclusive.
        /// </summary>
        /// <param name="probability">Value to examine.</param>
        private void EnsureLegalProbability(double probability)
        {
            if (probability < 0 || probability > 1)
            {
                throw new InvalidContentException("Probability values must be between 0 and 1 inclusive.");
            }
        }
    }
}