#region File Description
//-----------------------------------------------------------------------------
// DataModel.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Globalization;
using Yacht;
using System.Threading;
using System.Xml.Schema;


#endregion

namespace YachtServices
{
    #region Enums


    /// <summary>
    /// Possible game types.
    /// </summary>
    public enum GameTypes
    {
        Offline,
        Online
    }

    /// <summary>
    /// Possible message types sent by the game server.
    /// </summary>
    public enum MessageContentType
    {
        GameState = 1,
        SimpleType = 2,
        AvailableGames = 3,
        EndGameInformation = 4
    }


    #endregion

    /// <summary>
    /// A simple type message containing a name and a value.
    /// </summary>
    public class SimpleType : IXmlSerializable
    {
        #region Properties


        /// <summary>
        /// Message's value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Message's type.
        /// </summary>
        public string Name { get; set; }


        #endregion

        #region IXmlSerializable Members


        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"/>
        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads a serialized simple type message from a specified XML reader and populates the current instance.
        /// </summary>
        /// <param name="reader">The XML reader from which to read the data.</param>
        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();

            Name = reader.GetAttribute("Name");
            Value = reader.GetAttribute("Value");
        }

        /// <summary>
        /// Serializes the current instance and writes the result into a specified XML writer.
        /// </summary>
        /// <param name="writer">The XML writer into which the current instance is to be serialized.</param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("SimpleType");

            writer.WriteAttributeString("Name", Name);

            writer.WriteAttributeString("Value", Value);
        }


        #endregion
    }

    /// <summary>
    /// A message sent from the game server to a client.
    /// </summary>
    public class Message : IXmlSerializable
    {
        #region Properties


        /// <summary>
        /// The message's type.
        /// </summary>
        public MessageContentType ContentType { get; set; }

        /// <summary>
        /// The message's sequence number.
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// The message's body which contains XML serialized data.
        /// </summary>
        public IXmlSerializable Body { get; set; }


        #endregion

        #region IXmlSerializable Members


        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"/>
        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Populates the current instance according to serialized XML data.
        /// </summary>
        /// <param name="reader">XML reader from which to read the serialized message data.</param>
        public void ReadXml(XmlReader reader)
        {
            // Read the start element
            reader.Read();

            ContentType = (MessageContentType)Enum.Parse(typeof(MessageContentType), 
                reader.GetAttribute("ContentType"), true);

            SequenceNumber = int.Parse(reader.GetAttribute("SequenceNumber"));

            // Read body start element
            reader.Read();

            // Create the message according to the content type.
            switch (ContentType)
            {
                case MessageContentType.GameState:
                    Body = new GameState();
                    break;
                case MessageContentType.SimpleType:
                    Body = new SimpleType();
                    break;
                case MessageContentType.AvailableGames:
                    Body = new AvailableGames();
                    break;
                case MessageContentType.EndGameInformation:
                    Body = new EndGameInformation();
                    break;
                default:
                    break;
            }

            Body.ReadXml(reader);

            // Read body end element
            reader.Read();

            // Read message end element
            reader.Read();
        }

        /// <summary>
        /// Serialize the current instance and write the result into a specified XML writer.
        /// </summary>
        /// <param name="writer">The XML writer into which to write the serialized instance.</param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(GetType().Name);

            writer.WriteAttributeString("ContentType", ContentType.ToString());
            writer.WriteAttributeString("SequenceNumber", SequenceNumber.ToString());

            writer.WriteStartElement("Body");
            Body.WriteXml(writer);
            writer.WriteEndElement();

            writer.WriteEndElement();
        }


        #endregion
    }

    /// <summary>
    /// Contains player information.
    /// </summary>

    public class PlayerInformation
    {
        #region Properties


        /// <summary>
        /// The player's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The player's score card, containing 12 scores.
        /// </summary>
        public byte[] ScoreCard { get; set; }

        /// <summary>
        /// The player's total score.
        /// </summary>
        public int TotalScore { get; set; }

        /// <summary>
        /// The player's ID on the server.
        /// </summary>
        public int PlayerID { get; set; }

#if !WINDOWS_PHONE
        internal AIPlayerBehavior AIPlayer;
        internal Timer Timer;
        internal int TimeOutsCounter = 0;
        internal bool ToastMessageSent = false;
#endif


        #endregion

        #region Serialization Methods


        /// <summary>
        /// Populates the current instance according to serialized XML data.
        /// </summary>
        /// <param name="reader">XML reader from which to read the serialized player information data. Assumes the
        /// reader is positioned just before the serialized information.</param>
        /// <param name="gameType">The type of game the player participates in. This will be used to determine
        /// whether score card information needs to be deserialized.</param>
        public void Deserialize(XmlReader reader, GameTypes gameType)
        {
            // Read the start element
            reader.Read();

            Name = reader.GetAttribute("Name");
            PlayerID = int.Parse(reader.GetAttribute("ID"));
            TotalScore = int.Parse(reader.GetAttribute("Score"));

            if (gameType == GameTypes.Offline)
            {
                // Read score card information
                reader.Read();
                ScoreCard = new byte[12];
                reader.ReadContentAsBase64(ScoreCard, 0, 12);
            }
            else
            {
                // Read end element
                reader.Read();
            }            
        }

        /// <summary>
        /// Serialize the current instance and write the result into a specified XML writer.
        /// </summary>
        /// <param name="writer">The XML writer into which to write the serialized instance.</param>
        /// <param name="gameType">The type of game the player participates in. This will be used to determine
        /// whether score card information needs to be serialized.</param>
        public void Serialize(XmlWriter writer, GameTypes gameType)
        {
            // Write start element
            writer.WriteStartElement("PlayerInformation");
            writer.WriteAttributeString("Name", Name);
            writer.WriteAttributeString("ID", PlayerID.ToString());
            writer.WriteAttributeString("Score", TotalScore.ToString());

            if (gameType == GameTypes.Offline)
            {
                // Write score card information
                writer.WriteBase64(ScoreCard, 0, ScoreCard.Length);                                
            }

            // Write end element
            writer.WriteFullEndElement();
        }


        #endregion
    }

    /// <summary>
    /// Represents the entire state of a Yacht game.
    /// </summary>
    public class GameState : IXmlSerializable
    {        
        #region Fields

        /// <summary>
        /// A list of all player's participating in the game.
        /// </summary>
        public List<PlayerInformation> Players { get; set; }

        /// <summary>
        /// The amount of steps performed in the game.
        /// </summary>
        public byte StepsMade { get; set; }

        /// <summary>
        /// The game's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The ID of the currently active player.
        /// </summary>
        public int CurrentPlayer { get; set; }

        /// <summary>
        /// The game's unique identifier.
        /// </summary>
        public Guid GameID { get; set; }

        /// <summary>
        /// Whether or not the game has started.
        /// </summary>
        public bool IsStarted { get; set; }

        /// <summary>
        /// The game's type.
        /// </summary>
        public GameTypes GameType { get; set; }

#if !WINDOWS_PHONE
        /// <summary>
        /// Timer used to determine when the game should be deleted.
        /// </summary>
        public Timer TimerToDelete { get; set; }

        /// <summary>
        /// Sequence number for messages containing the game state.
        /// </summary>
        public int SequenceNumber { get; set; }
#endif


        #endregion

        #region Initialization


        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        public GameState()
        {
            Players = new List<PlayerInformation>();
            IsStarted = false;
        }


        #endregion

        #region Serialization Methods


        /// <summary>
        /// Not Implemented.
        /// </summary>
        /// <returns></returns>
        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Serialize the current instance and write the result into a specified XML writer.
        /// </summary>
        /// <param name="writer">The XML writer into which to write the serialized instance.</param>
        /// <param name="isOffline">Whether the player state represents an offline player.</param>
        public void Serialize(XmlWriter writer)
        {
            writer.WriteStartElement(GetType().Name);

            writer.WriteAttributeString("Type", GameType.ToString());
            writer.WriteAttributeString("IsStarted", IsStarted.ToString());
            writer.WriteAttributeString("GameID", GameID.ToString());
            writer.WriteAttributeString("CurrentPlayer", CurrentPlayer.ToString());
            writer.WriteAttributeString("Name", Name);
            writer.WriteAttributeString("StepsMade", StepsMade.ToString());
            writer.WriteAttributeString("NumberOfPlayers", Players.Count.ToString());

            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].Serialize(writer, GameType);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Reads a serialized GameState object. Unlike <see cref="ReadXml"/>, the reader is assumed to have
        /// already read the node which contains the start element of the network manager's representation. This will
        /// move the reader past the representation's end element.
        /// </summary>
        /// <param name="reader">The xml reader from which to read.</param>        
        public void Deserialize(XmlReader reader)
        {
            GameType = (GameTypes)Enum.Parse(typeof(GameTypes), reader.GetAttribute("Type"), false);
            GameID = new Guid(reader.GetAttribute("GameID"));
            IsStarted = bool.Parse(reader.GetAttribute("IsStarted"));
            CurrentPlayer = int.Parse(reader.GetAttribute("CurrentPlayer"));
            Name = reader.GetAttribute("Name");
            StepsMade = byte.Parse(reader.GetAttribute("StepsMade"));
            int numberOfPlayers = int.Parse(reader.GetAttribute("NumberOfPlayers"));

            for (int i = 0; i < numberOfPlayers; i++)
            {
                PlayerInformation playerInformation = new PlayerInformation();
                playerInformation.Deserialize(reader, GameType);
                Players.Add(playerInformation);
            }

            // Read the end element
            reader.Read();
        }

        /// <summary>
        /// Populates the current instance according to serialized XML data.
        /// </summary>
        /// <param name="reader">XML reader from which to read the serialized message data.</param>
        public void ReadXml(XmlReader reader)
        {
            // Read the start element
            reader.Read();

            Deserialize(reader);
        }

        /// <summary>
        /// Serialize the current instance and write the result into a specified XML writer.
        /// </summary>
        /// <param name="writer">The XML writer into which to write the serialized instance.</param>
        public void WriteXml(XmlWriter writer)
        {
            Serialize(writer);
        }

        /// <summary>
        /// The name of the root element in the object's XML representation.
        /// </summary>
        public string RootName
        {
            get
            {
                return "GameState";
            }
        }

        #endregion
    }

    /// <summary>
    /// Represent yacht step
    /// </summary>
    public class YachtStep : IXmlSerializable
    {
        #region Fields


        /// <summary>
        /// The index of the player who performed the move.
        /// </summary>
        public int PlayerIndex { get; set; }

        /// <summary>
        /// The index of the score line which was set during this move.
        /// </summary>
        public int ScoreLine { get; set; }

        /// <summary>
        /// The score that was set during this move.
        /// </summary>
        public byte Score { get; set; }        

        /// <summary>
        /// The step's number in the game.
        /// </summary>
        public int StepNumber { get; set; }


        #endregion      

        #region Initialization


        /// <summary>
        /// Creates a new Yacht step instance.
        /// </summary>
        /// <param name="scoreLine">The index of the score line which was set during this move.</param>
        /// <param name="score">The score that was set during this move.</param>
        /// <param name="playerIndex">The index of the player who performed the move.</param>
        /// <param name="stepNumber">The step's number in the game.</param>
        public YachtStep(int scoreLine, byte score, int playerIndex, int stepNumber)
        {
            ScoreLine = scoreLine;
            Score = score;
            PlayerIndex = playerIndex;
            StepNumber = stepNumber;
        }


        #endregion

        #region IXmlSerializable Members


        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <returns></returns>
        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Populates the current instance according to serialized XML data.
        /// </summary>
        /// <param name="reader">XML reader from which to read the serialized message data.</param>
        public void ReadXml(XmlReader reader)
        {
            PlayerIndex = int.Parse(reader.GetAttribute("PlayerIndex"));
            ScoreLine = int.Parse(reader.GetAttribute("ScoreLine"));
            Score = byte.Parse(reader.GetAttribute("Score"));
            StepNumber = int.Parse(reader.GetAttribute("StepNumber"));
        }

        /// <summary>
        /// Serialize the current instance and write the result into a specified XML writer.
        /// </summary>
        /// <param name="writer">The XML writer into which to write the serialized instance.</param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("YachtStep");
            writer.WriteAttributeString("PlayerIndex", PlayerIndex.ToString());
            writer.WriteAttributeString("ScoreLine", ScoreLine.ToString());
            writer.WriteAttributeString("Score", Score.ToString());
            writer.WriteAttributeString("StepNumber", StepNumber.ToString());
        }


        #endregion
    }

    /// <summary>
    /// Represent game information
    /// </summary>
    public struct GameInformation
    {
        public Guid GameID;
        public string Name;
    }

    /// <summary>
    /// Represent a list of available games received from the server.
    /// </summary>
    public class AvailableGames : IXmlSerializable
    {
        #region Fields


        /// <summary>
        /// The list of available games.
        /// </summary>
        public List<GameInformation> Games { get; set; }


        #endregion

        #region Initialization


        /// <summary>
        /// Creates a new available games instance.
        /// </summary>
        public AvailableGames()
        {
            Games = new List<GameInformation>();
        }


        #endregion

        #region IXmlSerializable Members


        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Populates the current instance according to serialized XML data.
        /// </summary>
        /// <param name="reader">XML reader from which to read the serialized message data.</param>
        public void ReadXml(XmlReader reader)
        {
            // Read start element
            reader.Read();

            int count = int.Parse(reader.GetAttribute("Count"));

            for (int i = 0; i < count; i++)
            {
                reader.Read();
                GameInformation gi = new GameInformation();
                gi.GameID = new Guid(reader.GetAttribute("GameID"));
                gi.Name = reader.GetAttribute("Name");
                Games.Add(gi);

                // A game element does not have an individual end tag
            }

            // Read end element
            reader.Read();
        }

        /// <summary>
        /// Serialize the current instance and write the result into a specified XML writer.
        /// </summary>
        /// <param name="writer">The XML writer into which to write the serialized instance.</param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(GetType().Name);

            writer.WriteAttributeString("Count", Games.Count.ToString());

            for (int i = 0; i < Games.Count; i++)
            {
                writer.WriteStartElement("Game");
                writer.WriteAttributeString("GameID", Games[i].GameID.ToString());
                writer.WriteAttributeString("Name", Games[i].Name);
                writer.WriteEndElement();
            }

            writer.WriteFullEndElement();
        }


        #endregion
    }


    /// <summary>
    /// Contains information about the player who won the game.
    /// </summary>
    public class EndGameInformation : IXmlSerializable
    {
        #region Fields


        /// <summary>
        /// Score card information of the victorious player.
        /// </summary>
        public byte[] ScoreCard { get; set; }

        /// <summary>
        /// The ID of the victorious player.
        /// </summary>
        public int PlayerID { get; set; }


        #endregion

        #region IXmlSerializable Members


        /// <summary>
        /// Not Implemented.
        /// </summary>
        /// <returns></returns>
        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Populates the current instance according to serialized XML data.
        /// </summary>
        /// <param name="reader">XML reader from which to read the serialized message data.</param>
        public void ReadXml(XmlReader reader)
        {
            reader.Read();
            PlayerID = int.Parse(reader.GetAttribute("ID"));
            ScoreCard = new byte[12];
            reader.ReadStartElement();
            reader.ReadContentAsBinHex(ScoreCard, 0, 12);
        }

        /// <summary>
        /// Serialize the current instance and write the result into a specified XML writer.
        /// </summary>
        /// <param name="writer">The XML writer into which to write the serialized instance.</param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("EndGameState");
            writer.WriteAttributeString("ID", PlayerID.ToString());
            writer.WriteBinHex(ScoreCard, 0, ScoreCard.Length);
        }


        #endregion
    }
}
