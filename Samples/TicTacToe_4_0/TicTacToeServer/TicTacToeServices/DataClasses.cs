#region File Description
//-----------------------------------------------------------------------------
// DataClasses.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;


#endregion

namespace TicTacToeServices
{
    /// <summary>
    /// Possible game states.
    /// </summary>
    public enum TicTacToeState
    {
        XPlayerTurn,
        OPlayerTurn,
        XPlayerWin,
        OPlayerWin,
        Tie
    }

    /// <summary>
    /// Denotes different message types.
    /// </summary>
    public enum MessageContentType
    {
        GameState,
       TicTacToeMove
    }

    /// <summary>
    /// A message sent to the client.
    /// </summary>
    public class Message : IXmlSerializable
    {
        /// <summary>
        /// Message's type.
        /// </summary>
        public MessageContentType ContentType { get; set; }

        /// <summary>
        /// Message body.
        /// </summary>
        public IXmlSerializable Body { get; set; }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"/>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads data into the Message object.
        /// </summary>
        /// <param name="reader">The xml reader used to read message data.</param>
        public void ReadXml(XmlReader reader)
        {
            reader.Read();
            reader.ReadStartElement();
            ContentType = (MessageContentType)reader.ReadElementContentAsInt();
            reader.ReadStartElement();


            switch (ContentType)
            {
                case MessageContentType.GameState:
                    Body = new GameState();
                    break;
                case MessageContentType.TicTacToeMove:
                    Body = new TicTacToeMove();
                    break;
            }

            Body.ReadXml(reader);
            reader.ReadEndElement();

        }

        /// <summary>
        /// Writes the object's data.
        /// </summary>
        /// <param name="writer">Xml writer used to write the object's data.</param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("ContentType");
            writer.WriteValue((int)ContentType);
            writer.WriteEndElement();

            writer.WriteStartElement("Body");
            Body.WriteXml(writer);
            writer.WriteEndElement();

           writer.WriteEndElement();

        }
    }

    /// <summary>
    /// Represent the state of a single game.
    /// </summary>
    public class GameState : IXmlSerializable
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public GameState()
        {
            Board = new string[3][];
            Board[0] = new string[] { string.Empty, string.Empty, string.Empty };
            Board[1] = new string[] { string.Empty, string.Empty, string.Empty };
            Board[2] = new string[] { string.Empty, string.Empty, string.Empty };

            CurrentState = TicTacToeState.XPlayerTurn;

        }

        /// <summary>
        /// Represents the game board.
        /// </summary>
        public string[][] Board { get; set; }

        /// <summary>
        /// The game's current state.
        /// </summary>
        public TicTacToeState CurrentState { get; set; }

        /// <summary>
        /// Amount of steps performed during the game.
        /// </summary>
        public int StepsMade { get; set; }
        
        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"/>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads data into the Message object.
        /// </summary>
        /// <param name="reader">The xml reader used to read message data.</param>
        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("GameState");
            StepsMade = reader.ReadElementContentAsInt();
            CurrentState = (TicTacToeState)reader.ReadElementContentAsInt();
            
            int Rows, Columns;
            int.TryParse(reader.GetAttribute("Rows"), out Rows);
            int.TryParse(reader.GetAttribute("Columns"), out Columns);
            
            string[] boardData = reader.ReadElementContentAsString().Split('0');
           
            Board = new string[Rows][];

            for (int row = 0; row < Rows; row++)
            {
                Board[row] = new string[Columns];
                for (int column = 0; column < Columns; column++)
                {
                    Board[row][column] = boardData[row * Columns + column];
                }
            }
        }

        /// <summary>
        /// Writes the object's data.
        /// </summary>
        /// <param name="writer">Xml writer used to write the object's data.</param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("GameState");

            writer.WriteStartElement("StepMade");
            writer.WriteValue(StepsMade);
            writer.WriteEndElement();


            writer.WriteStartElement("CurrentState");
            writer.WriteValue((int)CurrentState);
            writer.WriteEndElement();

            // The game board is serialized using '0' as the separator between cell values.
            StringBuilder sb = new StringBuilder();
            for (int row = 0; row < Board.Length; row++)
            {
                for (int column = 0; column < Board[row].Length; column++)
                {
                    sb.Append(Board[row][column] + '0');
                }
            }
            writer.WriteStartElement("Board");
            writer.WriteAttributeString("Rows", Board.Length.ToString());
            writer.WriteAttributeString("Columns", Board[0].Length.ToString());
            writer.WriteString(sb.ToString());
            writer.WriteEndElement();
        }
    }

    /// <summary>
    /// Represents a single step in the game.
    /// </summary>
    public class TicTacToeMove : IXmlSerializable
    {
        /// <summary>
        /// The X position on the board.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// The Y position on the board.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// The player which performed the move.
        /// </summary>
        public string Player { get; set; }

        /// <summary>
        /// Who will make the next step
        /// </summary>
        public string GameFlow { get; set; }


        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"/>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads data into the Message object.
        /// </summary>
        /// <param name="reader">The xml reader used to read message data.</param>
        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            X = reader.ReadElementContentAsInt();
            Y = reader.ReadElementContentAsInt();
            Player = reader.ReadElementContentAsString();
            GameFlow = reader.ReadElementContentAsString();
        }

        /// <summary>
        /// Writes the object's data.
        /// </summary>
        /// <param name="writer">Xml writer used to write the object's data.</param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Move");

            writer.WriteStartElement("X");
            writer.WriteValue(X);
            writer.WriteEndElement();

            writer.WriteStartElement("Y");
            writer.WriteValue(Y);
            writer.WriteEndElement();

            writer.WriteStartElement("Player");
            writer.WriteValue(Player);
            writer.WriteEndElement();

            writer.WriteStartElement("GameFlow");
            writer.WriteValue(GameFlow);
            writer.WriteEndElement();

        }
    }

    /// <summary>
    /// Contains game related constants.
    /// </summary>
    public static class ConstData
    {
        public static string GameName = "Tic Tac Toe ";

        public static string XString = "X";

        public static string OString = "O";
    }
 
}
