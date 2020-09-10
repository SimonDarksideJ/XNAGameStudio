#region File Description
//-----------------------------------------------------------------------------
// IYachtService.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;

using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
#endregion

namespace YachtServices
{
    [ServiceContract]
    public interface IYachtService
    {
        [OperationContract]
        int Register(Uri clientURI, string name, int playerID);

        [OperationContract]
        void Unregister(int sessionID);

        [OperationContract]
        bool JoinGame(Guid gameID, int sessionID);

        [OperationContract]
        bool LeaveGame(int sessionID);

        [OperationContract]
        void GameStep(Guid gameID, int sessionID, int scoreLine, byte score, int player, int step);

        [OperationContract]
        byte[] GetGameState(Guid gameID, int sessionID);

        [OperationContract]
        byte[] GetAvailableGames(int sessionID);

        [OperationContract]
        byte[] NewGame(int sessionID, string name);

        [OperationContract]
        void ResetTimeout(Guid gameID, int sessionID);

        [OperationContract]
        byte[] GetScoreCard(int sessionID);
    }
}
