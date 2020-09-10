#region File Description
//-----------------------------------------------------------------------------
// Program.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.ServiceModel;
using TicTacToeServices;
#endregion


namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(TicTacToeService)))
            {

                host.Open();

                Console.WriteLine("TicTacToe server is up...");

                Console.WriteLine("Press 'Enter' key to exit");
                Console.ReadLine();
            }

            Console.WriteLine("Exiting....");
        }
    }
}
