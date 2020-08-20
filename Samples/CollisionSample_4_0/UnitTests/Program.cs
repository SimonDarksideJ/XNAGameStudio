//---------------------------------------------------------------------------------------------------------------------
// UnitTests.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//---------------------------------------------------------------------------------------------------------------------

namespace UnitTests
{
    /// <summary>
    /// Unit tests for BoundingOrientedBox and Triangle collision code.
    ///
    /// Tests are in a standalone console app rather than using the test framework,
    /// so they can be run run in Express editions.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            UnitTests u = new UnitTests();
            u.RunTests();

            if (u.TestsFailed > 0)
            {
                System.Environment.Exit(1);
            }
        }
    }
}
