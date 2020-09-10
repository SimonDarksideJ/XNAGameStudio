#region File Description
//-----------------------------------------------------------------------------
// YachtState.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using System.Text;
using YachtServices;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;


#endregion

namespace Yacht
{
    public class YachtState : IXmlSerializable
    {
        #region Properties


        /// <summary>
        /// The state of the ongoing game. Can be null if no game is in progress.
        /// </summary>
        public GameState YachGameState { get; set; }

        /// <summary>
        /// The state of the player's dice. Can be null if no game is in progress, or the player does not manage his
        /// own dice.
        /// </summary>
        public DiceState PlayerDiceState { get; set; }

        /// <summary>
        /// Network related state information for contacting the game server.
        /// </summary>
        public NetworkManager NetworkManagerState { get; set; }


        #endregion

        #region IXmlSerializer Members


        /// <summary>
        /// Not implemented
        /// </summary>
        /// <returns></returns>
        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generates a Yacht state from its XML representation.
        /// </summary>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> stream from which the object 
        /// is deserialized.</param>
        /// <remarks>This method updates the singleton instance of <see cref="NetworkManager"/> with data read
        /// from the XML representation.</remarks>
        public void ReadXml(XmlReader reader)
        {
            // Read the start element
            reader.Read();

            YachGameState = null;
            PlayerDiceState = null;
            NetworkManagerState = NetworkManager.Instance;

            // Read the start element of the first sub-element
            reader.Read();

            // Read the yacht state's sub-elements
            while (reader.IsStartElement())
            {
                if (reader.LocalName == typeof(GameState).Name)
                {
                    YachGameState = new GameState();
                    YachGameState.Deserialize(reader);
                }
                else if (reader.LocalName == typeof(DiceState).Name)
                {
                    PlayerDiceState = new DiceState();
                    PlayerDiceState.Deserialize(reader);
                }
                else if (reader.LocalName == typeof(NetworkManager).Name)
                {
                    NetworkManagerState.Deserialize(reader);
                }

                // Read the next sub-element start element
                reader.Read();
            }

            // Read the end element
            reader.Read();
        }

        /// <summary>
        /// Converts a Yacht state into its XML representation.
        /// </summary>
        /// <param name="reader">The <see cref="System.Xml.XmlWriter"/> stream into which the object 
        /// is serialized.</param>
        public void WriteXml(XmlWriter writer)
        {
            // Write the start element
            writer.WriteStartElement(GetType().Name);

            // Serialize the available yacht state sub-elements
            if (YachGameState != null)
            {
                YachGameState.WriteXml(writer);
            }
            if (PlayerDiceState != null)
            {
                PlayerDiceState.WriteXml(writer);
            }
            if (NetworkManagerState != null)
            {
                NetworkManagerState.WriteXml(writer);
            }

            // Write the end element
            writer.WriteEndElement();
        }


        #endregion
    }
}
