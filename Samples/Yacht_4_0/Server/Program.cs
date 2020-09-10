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
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using YachtServices;


#endregion

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(YachtService)))
            {
                // Open the host and listen for clients
                host.Open();

                Console.WriteLine("Yacht Service is up...");

                Console.WriteLine("Press enter to close the service");

                // Terminate the service when pressing "Enter"
                Console.ReadLine();

                Console.WriteLine("service is closed");
            }
        }
    }
}
