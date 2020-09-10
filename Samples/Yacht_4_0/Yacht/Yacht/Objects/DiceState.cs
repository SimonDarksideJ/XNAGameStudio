#region File Description
//-----------------------------------------------------------------------------
// DiceState.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml;


#endregion

namespace Yacht
{
    /// <summary>
    /// The state of a player's dice.
    /// </summary>
    public class DiceState : IXmlSerializable
    {
        #region Fields


        /// <summary>
        /// Amount of dice the player has.
        /// </summary>
        public const int DieAmount = 5;

        /// <summary>
        /// How many times the dice have been rolled.
        /// </summary>
        public int Rolls { get; set; }

        /// <summary>
        /// The number of the turn for which the dice state is valid.
        /// </summary>
        public int ValidForTurn { get; set; }

        Dice[] rollingDice = new Dice[DieAmount];

        /// <summary>
        /// Dice that the player is rolling.
        /// </summary>
        public Dice[] RollingDice
        {
            get
            {
                return rollingDice;
            }
            set
            {
                rollingDice = value;
            }
        }

        Dice[] holdingDice = new Dice[DieAmount];

        /// <summary>
        /// Dice that the player is holding.
        /// </summary>
        public Dice[] HoldingDice
        {
            get
            {
                return holdingDice;
            }
            set
            {
                holdingDice = value;
            }
        }


        #endregion

        #region IXmlSerializable Methods


        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads a serialized DiceState object.
        /// </summary>
        /// <param name="reader">The xml reader from which to read.</param>
        public void ReadXml(XmlReader reader)
        {
            // Read the start element
            reader.Read();

            Deserialize(reader);
        }

        /// <summary>
        /// Serializes the DiceState object.
        /// </summary>
        /// <param name="writer">The xml writer to use when writing.</param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(GetType().Name);

            writer.WriteAttributeString("Rolls", Rolls.ToString());
            writer.WriteAttributeString("ValidForTurn", ValidForTurn.ToString());

            // Serialize rolling dice
            writer.WriteStartElement("RollingDice");

            for (int i = 0; i < DieAmount; i++)
            {
                writer.WriteAttributeString("D" + i.ToString(),
                    RollingDice[i] == null ? "" : ((int)RollingDice[i].Value).ToString());
            }

            writer.WriteEndElement();

            // Serialize held dice
            writer.WriteStartElement("HoldingDice");

            for (int i = 0; i < DieAmount; i++)
            {
                writer.WriteAttributeString("D" + i.ToString(),
                    HoldingDice[i] == null ? "" : ((int)HoldingDice[i].Value).ToString());
            }

            writer.WriteEndElement();

            // End element for the "DiceState" element
            writer.WriteEndElement();
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Reads a serialized DiceState object. Unlike <see cref="ReadXml"/>, the reader is assumed to have
        /// already read the node which contains the start element of the network manager's representation. This will
        /// move the reader past the representation's end element.
        /// </summary>
        /// <param name="reader">The xml reader from which to read.</param>
        public void Deserialize(XmlReader reader)
        {
            // Get the amount of rolls performed
            Rolls = int.Parse(reader.GetAttribute("Rolls"));

            // Get the turn for which the state is valid
            ValidForTurn = int.Parse(reader.GetAttribute("ValidForTurn"));

            // Get the rolling dice
            reader.Read();
            RollingDice = new Dice[DieAmount];

            for (int i = 0; i < DieAmount; i++)
            {
                string value = reader.GetAttribute("D" + i.ToString());
                if (value != "")
                {
                    RollingDice[i] = new Dice();
                    RollingDice[i].Value = (DiceValue)int.Parse(value);
                }
            }

            // The rolling dice element does not close explicitly

            // Get the held dice
            reader.Read();
            HoldingDice = new Dice[DieAmount];

            for (int i = 0; i < DieAmount; i++)
            {
                string value = reader.GetAttribute("D" + i.ToString());
                if (value != "")
                {
                    HoldingDice[i] = new Dice();
                    HoldingDice[i].Value = (DiceValue)int.Parse(value);
                }
            }

            // The held dice element does not close explicitly

            // Read end element
            reader.Read();
        }


        #endregion
    }
}
