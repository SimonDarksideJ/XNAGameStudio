using System;

namespace Graphics3DSample
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Sample3DGraphics game = new Sample3DGraphics())
            {
                game.Run();
            }
        }
    }
#endif
}

